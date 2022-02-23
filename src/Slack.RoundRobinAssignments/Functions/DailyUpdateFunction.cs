using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Slack.RoundRobinAssignments.Extensions;
using Slack.RoundRobinAssignments.Model;
using Slack.RoundRobinAssignments.Slack;

namespace Slack.RoundRobinAssignments.Functions;

public class DailyUpdateFunction
{
    private readonly SlackMessageSender _slackMessageSender;

    public DailyUpdateFunction(SlackMessageSender slackMessageSender)
    {
        _slackMessageSender = slackMessageSender;
    }

    [FunctionName(nameof(DailyUpdate))]
    public async Task DailyUpdate(
        [TimerTrigger("0 0 8 * * 1-5",
            RunOnStartup = false,
            UseMonitor = true)]
        TimerInfo timerInfo,
        [DurableClient] IDurableEntityClient client,
        ILogger log)
    {
        var state = await StatefulHelpers.ReadState<Assignment>(client, StatefulHelpers.GetEntityId());
        if (state == null)
        {
            log.LogWarning("No state created for daily update to work");
            await _slackMessageSender.NotifySlackUsers(
                $"🚨🚨🚨 Something is not configured. Error doing DailyUpdate().",
                log);
            return;
        }

        log.LogInformation($"Person for today is {state.GetPersonForToday().Name}");
        await _slackMessageSender.NotifySlackUsers(
            $"👮‍♂️Today {state.GetPersonForToday().Name} is on the watch, watch out!",
            log);
    }
}