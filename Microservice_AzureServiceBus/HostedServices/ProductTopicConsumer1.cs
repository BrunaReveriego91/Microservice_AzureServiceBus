using Microservice_AzureServiceBus.Models;
using Microsoft.Azure.ServiceBus;
using System.Text;
using System.Text.Json;

namespace Microservice_AzureServiceBus.HostedServices
{
    public class ProductTopicConsumer1 : IHostedService
    {
        static SubscriptionClient subscriptionClient;
        private readonly IConfiguration _config;
        public ProductTopicConsumer1(IConfiguration config)
        {
            _config = config;
            var serviceBusConnection = _config.GetValue<string>("AzureServiceBus");
            subscriptionClient = new SubscriptionClient(serviceBusConnection, "stores", "stores-sub1");
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Starting Consumer - Queue");
            ProcesssMessageHander();
            return Task.CompletedTask;
        }

        private void ProcesssMessageHander()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken arg2)
        {
            Console.WriteLine("Processing Message - Queue");
            Console.WriteLine($"{DateTime.Now}");
            Console.WriteLine($"Received message: SequenceNumber: {message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            Product _product = JsonSerializer.Deserialize<Product>(message.Body);

            await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);

        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Stopping Consumer - Queue");
            await subscriptionClient.CloseAsync();
            await Task.CompletedTask;
        }
    }
}
