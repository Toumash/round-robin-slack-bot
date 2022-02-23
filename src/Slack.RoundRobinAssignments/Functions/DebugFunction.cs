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

namespace Slack.RoundRobinAssignments.Functions;

public class DebugFunction
{
    private readonly ILogger<DebugFunction> _logger;

    public DebugFunction(ILogger<DebugFunction> logger)
    {
        _logger = logger;
    }

    [FunctionName(nameof(Debug))]
    public async Task<IActionResult> Debug(
        [HttpTrigger(AuthorizationLevel.Function, "post")]
        HttpRequest req,
        [DurableClient] IDurableEntityClient client)
    {
        var command = req.Form["command"].First();
        var text = req.Form["text"].FirstOrDefault() ?? "";
        _logger.LogDebug($"Got debug request: command: '{command}' text: '{text}'");

        switch (command)
        {
            case "view":
                return new OkObjectResult(
                    await client.ReadState<Assignment>(StatefulHelpers.GetEntityId()));
            case "next":
                await client.SignalEntityAsync(StatefulHelpers.GetEntityId(), nameof(Assignment.Next));
                return new OkResult();
            case "set":
                await client.SignalEntityAsync(StatefulHelpers.GetEntityId(), nameof(Assignment.SetOptions),
                    text.Split(' ').ToList());
                return new OkResult();
            default: return new EmptyResult();
        }
    }
}