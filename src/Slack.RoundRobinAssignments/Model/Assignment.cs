using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Slack.RoundRobinAssignments.Model;

[JsonObject(MemberSerialization.OptIn)]
public partial class Assignment
{
    private readonly ILogger _log;
    [JsonProperty("value")] public int AssignedPersonRowId { get; set; }

    [JsonProperty("options")] public List<string> Options { get; set; }

    public Assignment()
    {
    }

    public Assignment(ILogger log)
    {
        _log = log;
    }

    public AssignmentReadModel Next()
    {
        AssignedPersonRowId++;
        if (AssignedPersonRowId >= Options.Count) AssignedPersonRowId = 0;
        _log.LogInformation($"State advanced one day ahead. Today's assignment is for {GetPersonForToday().Name}");
        return GetPersonForToday();
    }

    public AssignmentReadModel WhoIsNext()
    {
        var row = AssignedPersonRowId;
        row++;
        if (row >= Options.Count) row = 0;
        return AssignmentReadModel.Create(Options[row]);
    }

    public AssignmentReadModel GetPersonForToday()
    {
        return AssignmentReadModel.Create(Options[AssignedPersonRowId]);
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
        Options = new List<string> { "Tomasz", "Karol" };
    }

    public bool SetOptions(List<string> options)
    {
        if (!options.Any()) return false;
        Options = options;
        AssignedPersonRowId = 0;
        return true;
    }
}