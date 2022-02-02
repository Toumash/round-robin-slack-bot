using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;

namespace Slack.RoundRobinAssignments
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AssignmentContext
    {
        [JsonProperty("value")] public int AssignedPersonRowId { get; set; }

        [JsonProperty("options")] public List<string> Options { get; set; } = new List<string>();


        public Assignment Next()
        {
            AssignedPersonRowId++;
            if (AssignedPersonRowId >= Options.Count) AssignedPersonRowId = 0;

            Console.WriteLine($"Next person: {GetPersonForToday().Name}");
            return GetPersonForToday();
        }

        public Assignment WhoIsNext()
        {
            var row = AssignedPersonRowId;
            row++;
            if (row >= Options.Count) row = 0;
            return Assignment.Create(Options[row]);
        }

        public Assignment GetPersonForToday()
        {
            return Assignment.Create(Options[AssignedPersonRowId]);
        }

        public bool SetPersonForToday(string personName)
        {
            if (!Options.Contains(personName))
            {
                return false;
            }

            AssignedPersonRowId = Options.IndexOf(personName);
            return true;
        }

        public void Reset()
        {
            AssignedPersonRowId = 0;
        }

        public bool SetOptions(List<string> options)
        {
            if (!options.Any()) return false;
            Options = options;
            AssignedPersonRowId = 0;
            return true;
        }


        [FunctionName(nameof(AssignmentContext))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
        {
            return ctx.DispatchAsync<AssignmentContext>();
        }
    }
}