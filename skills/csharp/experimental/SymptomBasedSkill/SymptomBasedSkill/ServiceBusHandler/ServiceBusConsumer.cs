using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SymptomBasedSkill.ServiceBusHandler
{
    public interface IServiceBusConsumer
    {
        void RegisterOnMessageHandlerAndReceiveMessages();
        Task CloseQueueAsync();
    }

    public class ServiceBusConsumer : IServiceBusConsumer
    {
        private readonly IProcessData _processData;
        private readonly IConfiguration _configuration;
        private readonly QueueClient _queueClient;
        private const string QUEUE_NAME = "detectfacemask";
        private readonly ILogger _logger;

        public ServiceBusConsumer(IProcessData processData,
            IConfiguration configuration,
            ILogger<ServiceBusConsumer> logger)
        {
            _processData = processData;
            _configuration = configuration;
            _logger = logger;
            _queueClient = new QueueClient("Endpoint=sb://facemaskdetector.servicebus.windows.net/;SharedAccessKeyName=connectionstring;SharedAccessKey=3CUeyvAhPwWfvCL1x/z0F/+59vyK6ltMTSu72BVD5I8=", QUEUE_NAME);
        }

        public void RegisterOnMessageHandlerAndReceiveMessages()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            var messageBody = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            _processData.Process(messageBody);
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.LogError(exceptionReceivedEventArgs.Exception, "Message handler encountered an exception");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            _logger.LogDebug($"- Endpoint: {context.Endpoint}");
            _logger.LogDebug($"- Entity Path: {context.EntityPath}");
            _logger.LogDebug($"- Executing Action: {context.Action}");

            return Task.CompletedTask;
        }

        public async Task CloseQueueAsync()
        {
            await _queueClient.CloseAsync();
        }
    }
}
