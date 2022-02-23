namespace Slack.RoundRobinAssignments.Model;

public class Assignment
{
    public string Name { get; private set; }

    public static Assignment Create(string name)
    {
        return new Assignment { Name = name };
    }
}