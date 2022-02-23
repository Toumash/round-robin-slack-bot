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

public class StartFunction
{
    private readonly ILogger<StartFunction> _logger;

    public StartFunction(ILogger<StartFunction> logger)
    {
        _logger = logger;
    }

    [FunctionName(nameof(Start))]
    public async Task<IActionResult> Start(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]
        HttpRequest req,
        [DurableClient] IDurableOrchestrationClient starter,
        [DurableClient] IDurableEntityClient client)
    {
        await client.SignalEntityAsync(StatefulHelpers.GetEntityId(), nameof(Assignment.Reset));
        _logger.LogInformation("Entity State has been reset");

        const string instanceId = "staticId";
        await starter.StartNewAsync(nameof(OrchestratorFunction.AssignmentOrchestration), instanceId);
        _logger.LogInformation($"Started with OrchestratorId= {instanceId}");

        return starter.CreateCheckStatusResponse(req, instanceId);
    }
}