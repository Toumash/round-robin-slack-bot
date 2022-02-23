using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Slack.RoundRobinAssignments.Slack;

public interface ISlackMessageSender
{
    Task NotifySlackUsers(string message);

    Task RespondAsASlackMessage(string message, string slackTokenUrl,
        bool ephemeral = false);
}

public class SlackMessageSender : ISlackMessageSender
{
    private readonly ILogger<SlackMessageSender> _logger;
    private readonly RestClient _restClient;
    private readonly string _webhookEndpoint;

    public SlackMessageSender(ILogger<SlackMessageSender> logger)
    {
        _logger = logger;
        _restClient = new RestClient();
        _webhookEndpoint = Environment.GetEnvironmentVariable("SLACK_WEBHOOK"); // FIXME: move to startup.cs
        if (_webhookEndpoint == null) throw new ApplicationException("No slack webhook configured");
    }

    public async Task NotifySlackUsers(string message)
    {
        var request = new RestRequest(_webhookEndpoint, Method.Post);
        request.AddJsonBody(new
        {
            text = message
        });
        var response = await _restClient.ExecuteAsync(request);
        _logger.LogInformation(
            $"message: {message}. Response: Success={response.IsSuccessful} HttpCode={response.StatusCode}");
    }

    public async Task RespondAsASlackMessage(string message, string slackTokenUrl,
        bool ephemeral = false)
    {
        if (slackTokenUrl == null) return;

        var request = new RestRequest(slackTokenUrl, Method.Post);
        request.AddJsonBody(new
        {
            response_type = ephemeral ? "ephemeral" : "in_channel",
            text = message
        });
        var response = await _restClient.ExecuteAsync(request);
        _logger.LogInformation(
            $"message: {message}. Response: Success={response.IsSuccessful} HttpCode={response.StatusCode}");
    }
}