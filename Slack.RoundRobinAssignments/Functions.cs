using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using static Slack.RoundRobinAssignments.StatefulHelpers;

namespace Slack.RoundRobinAssignments
{
    public static class Functions
    {
        [FunctionName(nameof(Start))]
        public static async Task<IActionResult> Start(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
            HttpRequest req, ILogger log,
            [DurableClient] IDurableOrchestrationClient starter,
            [DurableClient] IDurableEntityClient client)
        {
            await client.SignalEntityAsync(GetEntityId(), nameof(AssignmentContext.Reset));
            log.LogInformation($"Entity State has been reset");

            const string instanceId = "staticId";
            await starter.StartNewAsync(nameof(AssignmentOrchestration), instanceId);
            log.LogInformation($"Started with OrchestratorId= {instanceId}");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName(nameof(AssignmentOrchestration))]
        public static async Task AssignmentOrchestration(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {
            context.SignalEntity(GetEntityId(), nameof(AssignmentContext.Next));
            await WaitForTheNextDay(context);
            context.ContinueAsNew(null);
        }


        [FunctionName(nameof(DailyUpdate))]
        public static async Task DailyUpdate(
            [TimerTrigger("0 0 8 * * 1-5",
                RunOnStartup = false,
                UseMonitor = true)]
            TimerInfo timerInfo,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            var state = await ReadState<AssignmentContext>(client, GetEntityId());
            if (state == null)
            {
                log.LogWarning("No state created for daily update to work");
                await SlackHelper.NotifySlackUsers(
                    $"🚨🚨🚨 Something is not configured. Error doing DailyUpdate().",
                    log);
                return;
            }

            log.LogInformation($"Person for today is {state.GetPersonForToday().Name}");
            await SlackHelper.NotifySlackUsers(
                $"👮‍♂️Today {state.GetPersonForToday().Name} is on the watch, watch out!",
                log);
        }

        [FunctionName(nameof(ReceiveCommands))]
        public static async Task<IActionResult> ReceiveCommands(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequest req,
            [DurableClient] IDurableEntityClient client,
            ILogger log)
        {
            var command = req.Form["command"].First();
            var text = req.Form["text"].First();
            var responseUrl = req.Form["response_url"].First();
            switch (command)
            {
                case "/alerts-today":
                {
                    if (text == "skip")
                    {
                        var state = await ReadState<AssignmentContext>(client, GetEntityId());
                        if (state == null)
                        {
                            log.LogWarning("No state created for daily update to work");
                            break;
                        }

                        await client.SignalEntityAsync(GetEntityId(), nameof(AssignmentContext.Next));
                        log.LogInformation(
                            $"Skipping {state.GetPersonForToday().Name}. Next in line is: {state.WhoIsNext().Name}");
                        await SlackHelper.RespondAsASlackMessage(
                            $"Skipped {state.GetPersonForToday().Name}. Today {state.WhoIsNext().Name} is on the watch 👮‍♂️🚔",
                            responseUrl, log);
                    }
                    else
                    {
                        var state = await ReadState<AssignmentContext>(client, GetEntityId());
                        if (state == null)
                        {
                            log.LogWarning("No state created for daily update to work");
                            break;
                        }

                        log.LogInformation(
                            $"Today {state.GetPersonForToday().Name} is on the watch 👮‍♂️🚔");
                        await SlackHelper.RespondAsASlackMessage(
                            $"Today {state.GetPersonForToday().Name} is on the watch 👮‍♂️🚔",
                            responseUrl, log);
                    }

                    break;
                }
                case "/alerts-options":
                {
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        var state = await ReadState<AssignmentContext>(client, GetEntityId());
                        if (state == null)
                        {
                            log.LogWarning("No state created for daily update to work");
                            break;
                        }

                        await SlackHelper.RespondAsASlackMessage(
                            "Order: " + string.Join(" ", state.Options),
                            responseUrl, log);
                        log.LogInformation($"Order: [{string.Join(" ", state.Options)}]");
                        break;
                    }

                    var options = text.Split(' ').ToList();
                    await client.SignalEntityAsync(GetEntityId(), nameof(AssignmentContext.SetOptions), options);
                    await SlackHelper.RespondAsASlackMessage("New options:" + string.Join(" ", options),
                        responseUrl, log);
                    log.LogInformation($"Set options: [{string.Join(" ", options)}]");
                    break;
                }
            }

            return new OkResult();
        }
    }
}
