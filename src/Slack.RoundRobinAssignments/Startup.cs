using Slack.RoundRobinAssignments;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Slack.RoundRobinAssignments.Slack;


[assembly: FunctionsStartup(typeof(Startup))]

namespace Slack.RoundRobinAssignments
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddTransient<SlackMessageSender, SlackMessageSender>();
        }
    }
}