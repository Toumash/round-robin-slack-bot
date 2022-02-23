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
    private readonly ILogger<DailyUpdateFunction> _logger;
    private readonly ISlackMessageSender _slackMessageSender;

    public DailyUpdateFunction(ISlackMessageSender slackMessageSender, ILogger<DailyUpdateFunction> logger)
    {
        _slackMessageSender = slackMessageSender;
        _logger = logger;
    }

    [FunctionName(nameof(DailyUpdate))]
    public async Task DailyUpdate(
        [TimerTrigger("0 0 7 * * 1-5",
#if DEBUG
            RunOnStartup = true,
#endif
            UseMonitor = true)]
        TimerInfo timerInfo,
        [DurableClient] IDurableEntityClient client)
    {
        var state = await client.ReadState<Assignment>(StatefulHelpers.GetEntityId());
        if (state == null)
        {
            _logger.LogWarning("No state created for daily update to work");
            await _slackMessageSender.NotifySlackUsers(
                "🚨🚨🚨 Something is not configured. Error doing DailyUpdate().");
            return;
        }

        _logger.LogInformation($"Person for today is {state.GetPersonForToday().Name}");
        await _slackMessageSender.NotifySlackUsers(
            $"👮‍♂️Today {state.GetPersonForToday().Name} is on the watch, watch out!");
    }
}