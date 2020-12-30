using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessageLoggingService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private FileSystemWatcher _watcher;
        private MessageRequestService messages;
        private IConfiguration configuration;
        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.configuration = configuration;
        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            if (!Directory.Exists(configuration["FolderPath"])) Directory.CreateDirectory(configuration["FolderPath"]);
            _watcher = new FileSystemWatcher
            {
                Path = configuration["FolderPath"]
            };
            messages = new MessageRequestService(new HttpClient(), configuration);
            EnableFileChangeEvents();
            return base.StartAsync(cancellationToken);
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            messages.Dispose();
            _watcher.Dispose();
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!Directory.Exists(configuration["FolderPath"]))
                {
                    Directory.CreateDirectory(configuration["FolderPath"]);
                }
                _watcher.EnableRaisingEvents = true;
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
        private async void OnRenamed(object sender, RenamedEventArgs e)
        {
            try
            {
                string response = await messages.SendMessage(e.FullPath, FileChangeType.Renamed);
                _logger.LogInformation(response);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }
        }

        private async void OnChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                string response = await messages.SendMessage(e.FullPath, FileChangeType.Changed);
                _logger.LogInformation(response);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }
        }

        private async void OnDeleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                string response = await messages.SendMessage(e.FullPath, FileChangeType.Deleted);
                _logger.LogInformation(response);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }
        }

        private async void OnCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                string response = await messages.SendMessage(e.FullPath, FileChangeType.Created);
                _logger.LogInformation(response);
            }
            catch(Exception exception)
            {
                _logger.LogError(exception.Message);
            }
        }
        private async void OnErrorOccured(object sender, ErrorEventArgs e)
        {
            try
            {
                string response = await messages.SendMessageError(e.GetException());
                _logger.LogError(response);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message);
            }
        }
    }
}
