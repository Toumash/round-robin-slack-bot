using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Slack.RoundRobinAssignments.Commands;

namespace Slack.RoundRobinAssignments.Functions;

public class BotCommandsFunction
{
    private readonly IBotMessageHandler _botMessageHandler;

    public BotCommandsFunction(IBotMessageHandler botMessageHandler)
    {
        _botMessageHandler = botMessageHandler;
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

        var botMessage = new BotCommand(command, text, responseUrl);
        await _botMessageHandler.Handle(client, botMessage);
        return new OkResult();
    }
}