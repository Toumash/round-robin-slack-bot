using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Slack.RoundRobinAssignments.Extensions;
using Slack.RoundRobinAssignments.Model;
using Slack.RoundRobinAssignments.Slack;

namespace Slack.RoundRobinAssignments.Commands;

public interface IBotMessageHandler
{
    Task Handle(IDurableEntityClient client, BotCommand botCommand);
}

public class BotMessageHandler : IBotMessageHandler
{
    private readonly ILogger<BotMessageHandler> _logger;
    private readonly ISlackMessageSender _slackMessageSender;

    public BotMessageHandler(ILogger<BotMessageHandler> logger, ISlackMessageSender slackMessageSender)
    {
        _logger = logger;
        _slackMessageSender = slackMessageSender;
    }


    private async Task SetOptions(IDurableEntityClient client, string text, string responseUrl)
    {
        var options = text.Split(' ').ToList();
        await client.SignalEntityAsync(StatefulHelpers.GetEntityId(), nameof(Assignment.SetOptions), options);
        await _slackMessageSender.RespondAsASlackMessage(
            $"New options: {string.Join(" ", options)}. Tomorrow is {options.First()} on watch 👮‍♂️",
            responseUrl, true);
        _logger.LogInformation($"Set options: [{string.Join(" ", options)}]");
    }

    private async Task ShowAllOptions(IDurableEntityClient client, string responseUrl)
    {
        var state = await client.ReadState<Assignment>(StatefulHelpers.GetEntityId());
        if (state == null)
        {
            _logger.LogWarning("No state created for daily update to work");
            return;
        }

        await _slackMessageSender.RespondAsASlackMessage(
            $"Current order:{string.Join(" ", state.Options)}. Today is {state.GetPersonForToday().Name}",
            responseUrl, true);
        _logger.LogInformation($"Order: [{string.Join(" ", state.Options)}]");
    }

    private async Task ShowTodayPerson(IDurableEntityClient client, string responseUrl)
    {
        var state = await client.ReadState<Assignment>(StatefulHelpers.GetEntityId());
        if (state == null)
        {
            _logger.LogWarning("No state created for daily update to work");
            return;
        }

        _logger.LogInformation(
            $"Today {state.GetPersonForToday().Name} is on the watch 👮‍♂️🚔");
        await _slackMessageSender.RespondAsASlackMessage(
            $"Today {state.GetPersonForToday().Name} is on the watch 👮‍♂️🚔",
            responseUrl);
    }

    private async Task SkipTodayPerson(IDurableEntityClient client, string responseUrl)
    {
        var state = await client.ReadState<Assignment>(StatefulHelpers.GetEntityId());
        if (state == null)
        {
            _logger.LogWarning("No state created for daily update to work");
            return;
        }

        await client.SignalEntityAsync(StatefulHelpers.GetEntityId(), nameof(Assignment.Next));
        _logger.LogInformation(
            $"Skipping {state.GetPersonForToday().Name}. Next in line is: {state.WhoIsNext().Name}");
        await _slackMessageSender.RespondAsASlackMessage(
            $"Skipped {state.GetPersonForToday().Name}. Today {state.WhoIsNext().Name} is on the watch 👮‍♂️🚔",
            responseUrl);
    }

    public Task Handle(IDurableEntityClient client, BotCommand botCommand) =>
        botCommand switch
        {
            { Command: "/alerts-today", Text: "skip" } =>
                SkipTodayPerson(client, botCommand.ResponseUrl),

            { Command: "/alerts-today", Text: "" } =>
                ShowTodayPerson(client, botCommand.ResponseUrl),

            { Command: "/alerts-options", Text: "" } =>
                ShowAllOptions(client, botCommand.ResponseUrl),

            { Command: "/alerts-options" } when !string.IsNullOrEmpty(botCommand.Text) =>
                SetOptions(client, botCommand.Text, botCommand.ResponseUrl),
            
            _ => throw new NotImplementedException()
        };
}