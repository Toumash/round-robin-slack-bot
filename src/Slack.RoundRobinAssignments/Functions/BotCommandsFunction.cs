using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Slack.RoundRobinAssignments.Extensions;
using Slack.RoundRobinAssignments.Model;
using Slack.RoundRobinAssignments.Slack;

namespace Slack.RoundRobinAssignments.Functions;

public class BotCommandsFunction
{
    private readonly SlackMessageSender _slackMessageSender;

    public BotCommandsFunction(SlackMessageSender slackMessageSender)
    {
        _slackMessageSender = slackMessageSender;
    }

    [FunctionName(nameof(ReceiveCommands))]
    public async Task<IActionResult> ReceiveCommands(
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
                    var state = await StatefulHelpers.ReadState<Assignment>(client, StatefulHelpers.GetEntityId());
                    if (state == null)
                    {
                        log.LogWarning("No state created for daily update to work");
                        break;
                    }

                    await client.SignalEntityAsync(StatefulHelpers.GetEntityId(), nameof(Assignment.Next));
                    log.LogInformation(
                        $"Skipping {state.GetPersonForToday().Name}. Next in line is: {state.WhoIsNext().Name}");
                    await _slackMessageSender.RespondAsASlackMessage(
                        $"Skipped {state.GetPersonForToday().Name}. Today {state.WhoIsNext().Name} is on the watch 👮‍♂️🚔",
                        responseUrl, log);
                }
                else
                {
                    var state = await StatefulHelpers.ReadState<Assignment>(client, StatefulHelpers.GetEntityId());
                    if (state == null)
                    {
                        log.LogWarning("No state created for daily update to work");
                        break;
                    }

                    log.LogInformation(
                        $"Today {state.GetPersonForToday().Name} is on the watch 👮‍♂️🚔");
                    await _slackMessageSender.RespondAsASlackMessage(
                        $"Today {state.GetPersonForToday().Name} is on the watch 👮‍♂️🚔",
                        responseUrl, log);
                }

                break;
            }
            case "/alerts-options":
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    var state = await StatefulHelpers.ReadState<Assignment>(client, StatefulHelpers.GetEntityId());
                    if (state == null)
                    {
                        log.LogWarning("No state created for daily update to work");
                        break;
                    }

                    await _slackMessageSender.RespondAsASlackMessage(
                        $"Current order:{string.Join(" ", state.Options)}. Today is {state.GetPersonForToday().Name}",
                        responseUrl, log, ephemeral: true);
                    log.LogInformation($"Order: [{string.Join(" ", state.Options)}]");
                    break;
                }

                var options = text.Split(' ').ToList();
                await client.SignalEntityAsync(StatefulHelpers.GetEntityId(), nameof(Assignment.SetOptions), options);
                await _slackMessageSender.RespondAsASlackMessage(
                    $"New options: {string.Join(" ", options)}. Tomorrow is {options.First()} on watch 👮‍♂️",
                    responseUrl, log, ephemeral: true);
                log.LogInformation($"Set options: [{string.Join(" ", options)}]");
                break;
            }
            case "/alerts-local-test":
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    var state = await StatefulHelpers.ReadState<Assignment>(client, StatefulHelpers.GetEntityId());
                    if (state == null)
                    {
                        log.LogWarning("No state created for daily update to work");
                        break;
                    }

                    await _slackMessageSender.RespondAsASlackMessage(
                        "Order: " + string.Join(" ", state.Options),
                        responseUrl, log);
                    log.LogInformation($"Order: [{string.Join(" ", state.Options)}]");
                    break;
                }

                var options = text.Split(' ').ToList();
                await client.SignalEntityAsync(StatefulHelpers.GetEntityId(), nameof(Assignment.SetOptions), options);
                await _slackMessageSender.RespondAsASlackMessage("New options:" + string.Join(" ", options),
                    responseUrl, log);
                log.LogInformation($"Set options: [{string.Join(" ", options)}]");
                break;
            }
        }

        return new OkResult();
    }
}