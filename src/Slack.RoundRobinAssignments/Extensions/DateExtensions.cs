using System;
using System.Linq;

namespace Slack.RoundRobinAssignments.Extensions;

public static class DateExtensions
{
    public static DateTime GetNextWorkingDay(DateTime currentDay)
    {
        var nextTick = currentDay.Date.AddDays(1);

        while (new[] { DayOfWeek.Sunday, DayOfWeek.Saturday }.Contains(nextTick.DayOfWeek))
        {
            nextTick = nextTick.AddDays(1);
        }

        return nextTick;
    }
}