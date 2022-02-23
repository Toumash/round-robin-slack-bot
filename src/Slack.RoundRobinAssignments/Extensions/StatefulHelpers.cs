using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Slack.RoundRobinAssignments.Model;

namespace Slack.RoundRobinAssignments.Extensions;

public static class StatefulHelpers
{
    public static async Task WaitForNextDay(IDurableOrchestrationContext context)
    {
        var nextTick = DateExtensions.GetNextWorkingDay(context.CurrentUtcDateTime);

        await context.CreateTimer(nextTick, CancellationToken.None);
    }

    public static async Task<T> ReadState<T>(this IDurableEntityClient client, EntityId entityId)
    {
        var entityStateInfo = await client.ReadEntityStateAsync<T>(entityId);
        return entityStateInfo.EntityState;
    }

    public static EntityId GetEntityId(string entityKey = "singleton")
    {
        // always the same, because we're gonna use it only for one customer
        // if you want to use function with multiple state, change the entityKey
        var entityId = new EntityId(nameof(Assignment), entityKey);
        return entityId;
    }
}