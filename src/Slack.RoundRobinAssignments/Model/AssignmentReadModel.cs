namespace Slack.RoundRobinAssignments.Model;

public class AssignmentReadModel
{
    public string Name { get; private init; }

    public static AssignmentReadModel Create(string name)
    {
        return new AssignmentReadModel { Name = name };
    }
}