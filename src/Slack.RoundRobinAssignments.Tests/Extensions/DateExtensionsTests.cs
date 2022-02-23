using System;
using Shouldly;
using Slack.RoundRobinAssignments.Extensions;
using Xunit;

namespace Slack.RoundRobinAssignments.Tests.Extensions;

public class DateExtensionsTests
{
    [Fact]
    public void GetNextWorkingDay_GivenMonday_ShouldReturn_Tuesday()
    {
        DateExtensions.GetNextWorkingDay(new DateTime(2022, 01, 31)).Date.ShouldBe(new DateTime(2022, 02, 01));
    }

    [Fact]
    public void GetNextWorkingDay_GivenFriday_ShouldReturn_Monday()
    {
        DateExtensions.GetNextWorkingDay(new DateTime(2022, 02, 4)).Date.ShouldBe(new DateTime(2022, 02, 07));
    }
}