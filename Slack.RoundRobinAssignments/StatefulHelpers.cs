﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Slack.RoundRobinAssignments
{
    public static class StatefulHelpers
    {
        public static async Task WaitForTheNextDay(IDurableOrchestrationContext context)
        {
            var nextCleanup = context.CurrentUtcDateTime.AddSeconds(60); // TODO: change to next day
            await context.CreateTimer(nextCleanup, CancellationToken.None);
        }

        public static async Task<T> ReadState<T>(IDurableEntityClient client, EntityId entityId)
        {
            var entityStateInfo = await client.ReadEntityStateAsync<T>(entityId);
            return entityStateInfo.EntityState;
        }

        public static EntityId GetEntityId(string entityKey = "singleton")
        {
            // always the same, because we're gonna use it only for one customer
            // if you want to use function with multiple state, change the entityKey
            var entityId = new EntityId(nameof(AssignmentContext), entityKey);
            return entityId;
        }
    }
}