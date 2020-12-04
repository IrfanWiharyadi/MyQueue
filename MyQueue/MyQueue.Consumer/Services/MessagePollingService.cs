
using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyQueue.Consumer.Model;

namespace MyQueue.Consumer.Services
{
    public class MessagePollingService : BackgroundService
    {
        private readonly ILogger<MessagePollingService> _logger;
        private readonly IOptions<SqsSetting> _setting;
        private readonly IAmazonSQS _sqsClient;

        public MessagePollingService(ILogger<MessagePollingService> logger, IOptions<SqsSetting> setting, IAmazonSQS sqsClient)
        {
            _logger = logger;
            _setting = setting;
            _sqsClient = sqsClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessMessages();

                await Task.Delay(2000, stoppingToken);
            }
        }

        private async Task ProcessMessages()
        {
            try
            {
                var queueUrlResponse = await _sqsClient.GetQueueUrlAsync(_setting.Value.QueueName);
                var request = new ReceiveMessageRequest(queueUrlResponse.QueueUrl);

                var response = await _sqsClient.ReceiveMessageAsync(request);

                response.Messages.ForEach(async x =>
                {
                    try
                    {
                        Console.WriteLine($"Message Received: {x.Body}");
                    }
                    finally
                    {
                        await _sqsClient.DeleteMessageAsync(new DeleteMessageRequest(queueUrlResponse.QueueUrl, x.ReceiptHandle));
                    }
                });
            }

            catch (Exception ex)
            {
                string errMessage = $"Failed when receiving message from [{_setting.Value.QueueName}] queue. {ex.Message}";
                _logger.LogError(ex, errMessage);
            }
        }
    }
}
