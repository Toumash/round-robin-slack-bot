using System;
using System.Diagnostics;
using System.Threading.Tasks;
using RestSharp;

namespace Slack.RoundRobinAssignments
{
    public static class SlackHelper
    {
        private static readonly RestClient RestClient = new RestClient();

        public static void NotifySlackUsers(Assignment personOfTheDay)
        {
        }

        public static async Task RespondAsASlackMessage(string message, string slackTokenUrl)
        {
            var request = new RestRequest(slackTokenUrl, Method.Post);
            request.AddJsonBody(new
            {
                response_type = "ephemeral", 
                text = message
            });
            var response = await RestClient.ExecuteAsync(request);
            Console.WriteLine(response.Content);
        }
    }
}