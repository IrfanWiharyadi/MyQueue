using System;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyQueue.Publisher.Model;

namespace MyQueue.Publisher.Services
{
    public interface IMessageService
    {
        Task<bool> PublishAsync(string message);
    }

    public class MessageService: IMessageService
    {
        private readonly IAmazonSQS _amazonSqsClient;
        private readonly ILogger<MessageService> _logger;
        private readonly IOptions<SqsConfig> _sqsConfig;

        public MessageService(IAmazonSQS amazonSqsClient, ILogger<MessageService> logger, IOptions<SqsConfig> sqsConfig)
        {
            _amazonSqsClient = amazonSqsClient;
            _logger = logger;
            _sqsConfig = sqsConfig;
        }

        public async Task<bool> PublishAsync(string message)
        {
            string awsQueueName = _sqsConfig.Value.QueueNameFormat;

            try
            {
                string queueUrl = await GetQueueAsync(awsQueueName);

                var request = new SendMessageRequest
                {
                    MessageBody = message,
                    QueueUrl = queueUrl,
                    MessageGroupId = "Any",
                    MessageDeduplicationId = Guid.NewGuid().ToString()
                };
                var response = await _amazonSqsClient.SendMessageAsync(request);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }

        private async Task<string> GetQueueAsync(string awsQueueName)
        {
            try
            {
                return (await _amazonSqsClient.GetQueueUrlAsync(awsQueueName)).QueueUrl;
            }
            catch (QueueDoesNotExistException ex)
            {
                return await CreateQueueAsync(awsQueueName);
            }
        }

        private async Task<string> CreateQueueAsync(string awsQueueName)
        {
            var createQueueRequest = new CreateQueueRequest(awsQueueName);
            return (await _amazonSqsClient.CreateQueueAsync(createQueueRequest)).QueueUrl;
        }
    }
}
