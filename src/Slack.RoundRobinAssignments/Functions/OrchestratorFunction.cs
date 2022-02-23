using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Slack.RoundRobinAssignments.Extensions;
using Slack.RoundRobinAssignments.Model;

namespace Slack.RoundRobinAssignments.Functions;

public static class OrchestratorFunction
{
    [FunctionName(nameof(AssignmentOrchestration))]
    public static async Task AssignmentOrchestration(
        [OrchestrationTrigger] IDurableOrchestrationContext context,
        ILogger log)
    {
        context.SignalEntity(StatefulHelpers.GetEntityId(), nameof(Assignment.Next));
        await StatefulHelpers.WaitForNextDay(context);
        context.ContinueAsNew(null);
    }
}