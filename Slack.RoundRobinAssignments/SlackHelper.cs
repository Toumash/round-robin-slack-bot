﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace Slack.RoundRobinAssignments
{
    public static class SlackHelper
    {
        private static readonly RestClient RestClient = new RestClient();

        public static async Task NotifySlackUsers(string message, ILogger log)
        {
            var slackWebhook = Environment.GetEnvironmentVariable("SLACK_WEBHOOK");
            var request = new RestRequest(slackWebhook, Method.Post);
            request.AddJsonBody(new
            {
                text = message
            });
            var response = await RestClient.ExecuteAsync(request);
            log.LogInformation(
                $"{nameof(NotifySlackUsers)} message: {message}. Response: Success={response.IsSuccessful} HttpCode={response.StatusCode.ToString()}");
        }

        public static async Task RespondAsASlackMessage(string message, string slackTokenUrl, ILogger log)
        {
            var request = new RestRequest(slackTokenUrl, Method.Post);
            request.AddJsonBody(new
            {
                response_type = "ephemeral",
                text = message
            });
            var response = await RestClient.ExecuteAsync(request);
            log.LogInformation(
                $"{nameof(NotifySlackUsers)} message: {message}. Response: Success={response.IsSuccessful} HttpCode={response.StatusCode.ToString()}");
        }
    }
}