using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Slack.RoundRobinAssignments;
using Slack.RoundRobinAssignments.Commands;
using Slack.RoundRobinAssignments.Slack;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Slack.RoundRobinAssignments;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddHttpClient();
        builder.Services.AddLogging();
        builder.Services.AddTransient<ISlackMessageSender, SlackMessageSender>();
        builder.Services.AddTransient<IBotMessageHandler, BotMessageHandler>();
    }
}