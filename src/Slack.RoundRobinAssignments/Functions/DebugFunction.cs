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

public static class DebugFunction
{
    [FunctionName(nameof(Debug))]
    public static async Task<IActionResult> Debug(
        [HttpTrigger(AuthorizationLevel.Function, "post")]
        HttpRequest req,
        [DurableClient] IDurableEntityClient client,
        ILogger log)
    {
        var command = req.Form["command"].First();
        var text = req.Form["text"].FirstOrDefault();
        switch (command)
        {
            case "view": return new OkObjectResult(await StatefulHelpers.ReadState<Assignment>(client, StatefulHelpers.GetEntityId()));
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