using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Slack.RoundRobinAssignments.Extensions;
using Slack.RoundRobinAssignments.Model;

namespace Slack.RoundRobinAssignments.Functions;

public static class StartFunction
{
    [FunctionName(nameof(Start))]
    public static async Task<IActionResult> Start(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
        HttpRequest req, ILogger log,
        [DurableClient] IDurableOrchestrationClient starter,
        [DurableClient] IDurableEntityClient client)
    {
        await client.SignalEntityAsync(StatefulHelpers.GetEntityId(), nameof(Assignment.Reset));
        log.LogInformation($"Entity State has been reset");

        const string instanceId = "staticId";
        await starter.StartNewAsync(nameof(OrchestratorFunction.AssignmentOrchestration), instanceId);
        log.LogInformation($"Started with OrchestratorId= {instanceId}");

        return starter.CreateCheckStatusResponse(req, instanceId);
    }
}