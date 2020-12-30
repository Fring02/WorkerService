using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MessageQueueApi.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MessageEmailService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IEmailSender sender;
        private HttpClient client;
        private IConfiguration configuration;
        public Worker(ILogger<Worker> logger, IEmailSender sender, IConfiguration configuration)
        {
            _logger = logger;
            this.sender = sender;
            this.configuration = configuration;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            client = new HttpClient();
            return base.StartAsync(cancellationToken);
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            client.Dispose();
            return base.StopAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var response = await client.GetAsync(configuration.GetSection("MessageRequests")["RetrieveEmail"]);
                try
                {
                   var message = await SendEmailMessage(configuration.GetSection("EmailConfiguration")["From"], response);
                    if (message != null)
                    {
                        _logger.LogInformation($"Message {message.Id} was sent to email workerservice@gmail.com");
                        await HandleMessage(message.Id);
                    }
                }
                catch(Exception e)
                {
                    _logger.LogError(e.Message);
                }
                await Task.Delay(5000, stoppingToken);
            }
        }


        private async Task<MessageDto> SendEmailMessage(string email, HttpResponseMessage response)
        {
            string emailResult = response.Content.ReadAsStringAsync().Result;
            try
            {
                var messageDto = JsonConvert.DeserializeObject<MessageDto>(emailResult);
                if (messageDto != null)
                {
                    _logger.LogInformation("Found message with id: " + messageDto.Id);
                    var message = new Message(new string[] { email }, "File status", messageDto.Text, string.Empty);
                    await sender.SendEmail(message);
                    return messageDto;
                } else
                {
                    _logger.LogWarning("Didn't found any unhandled message");
                    return null;
                }
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }

        private async Task<bool> HandleMessage(Guid id)
        {
            var handleResponse = await client.GetAsync(configuration.GetSection("MessageRequests")["Handle"] + id);
            string handleResult = await handleResponse.Content.ReadAsStringAsync();
            if (handleResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation("Handled message " + id);
                return true;
            }
            else
            {
                _logger.LogWarning("Can't handle message: " + handleResult);
                return false;
            }
        }
    }
}
