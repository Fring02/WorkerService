using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmailService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EmailSendingWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IEmailSender _sender;
        private FileSystemWatcher _watcher;
        private readonly string directoryPath = @"C:\Users\ASUS\Desktop\MyFolder";
        public Worker(ILogger<Worker> logger, IEmailSender sender)
        {
            _logger = logger;
            _sender = sender;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _watcher = new FileSystemWatcher
            {
                Path = directoryPath
            };
            EnableFileChangeEvents();
            return base.StartAsync(cancellationToken);
        }

        private async void OnRenamed(object sender, RenamedEventArgs e)
        {
            var message = new Message(new string[] { "workerservicetest@gmail.com" }, "Your file was renamed",
                $"File {e.OldName} was renamed to {e.Name}", e.FullPath);
            _logger.LogInformation($"Renamed file {e.OldName} to " + e.Name);
            _sender.SendEmail(message);
        }

        private async void OnChanged(object sender, FileSystemEventArgs e)
        {
            var message = new Message(new string[] { "workerservicetest@gmail.com" }, "Your file was changed",
                $"File {e.Name} was changed", e.FullPath);
            _logger.LogInformation("Changed file " + e.Name);
             _sender.SendEmail(message);
        }

        private async void OnDeleted(object sender, FileSystemEventArgs e)
        {
            var message = new Message(new string[] { "workerservicetest@gmail.com" }, "Your file was deleted",
                $"File {e.Name} was deleted", e.FullPath);
            _logger.LogWarning("Deleted file " + e.Name);
             _sender.SendEmail(message);
        }

        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            var message = new Message(new string[] { "workerservicetest@gmail.com" }, 
                "Your file was created", $"File {e.Name} was created", e.FullPath);
            _logger.LogInformation("Trying to send email for " + message.To.First());
             _sender.SendEmail(message);
        }
        private async void OnErrorOccured(object sender, ErrorEventArgs e)
        {
            var message = new Message(new string[] { "workerservicetest@gmail.com" },
               "Error, while dealing with file", e.GetException().Message, directoryPath);
            var exception = e.GetException();
            _logger.LogCritical(exception.Message + '\n' + exception.StackTrace);
            _sender.SendEmail(message);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                _watcher.EnableRaisingEvents = true;
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void EnableFileChangeEvents()
        {
            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Changed += OnChanged;
            _watcher.Renamed += OnRenamed;
            _watcher.Error += OnErrorOccured;
        }
    }
}
