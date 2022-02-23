using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace Slack.RoundRobinAssignments.Model;

public partial class Assignment
{
    [FunctionName(nameof(Assignment))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx, ILogger log)
    {
        return ctx.DispatchAsync<Assignment>(log);
    }
}