using MessageQueueApi.Models.Enums;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MessageLoggingService
{
    class MessageRequestService : IDisposable
    {
        private HttpClient client;
        private IConfiguration configuration;

        public MessageRequestService(HttpClient client, IConfiguration configuration)
        {
            this.client = client;
            this.configuration = configuration;
        }

        public void Dispose()
        {
            client.Dispose();
        }

        public async Task<string> SendMessage(string fileName, FileChangeType type)
        {
            string typeAction = Enum.GetName(typeof(FileChangeType), type).ToLower();
            var message = new
            {
                Type = MessageType.Email,
                Text = $"A file {fileName} was {typeAction}"
            };
            var data = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(configuration.GetSection("MessageRequests")["Add"], data);
            return response.Content.ReadAsStringAsync().Result;
        }

        public async Task<string> SendMessageError(Exception exception)
        {
            var message = new
            {
                MessageType = MessageType.Email,
                JsonContent = $"File error occured: {exception.Message}"
            };
            var data = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(configuration.GetSection("MessageRequests")["Add"], data);
            return response.Content.ReadAsStringAsync().Result;
        }
    }
}
