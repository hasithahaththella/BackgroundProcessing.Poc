using BackgroundProcessing.Core.Settings;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Amqp.Framing;
using BackgroundProcessing.Core;

namespace BackgroundProcessing.ServiceBusQueue
{
    public class ServiceBusQueueService : IServiceBusQueueService
    { 
        private readonly IAzureSettingsManager _configuration;
        private readonly WriteLine _logger;

        // the client that owns the connection and can be used to create senders and receivers
        ServiceBusClient client;

        // the sender used to publish messages to the queue
        ServiceBusSender sender;


        public ServiceBusQueueService(IAzureSettingsManager azureSettingsManager, WriteLine logger)
        {
            _configuration = azureSettingsManager;
            // The Service Bus client types are safe to cache and use as a singleton for the lifetime
            // of the application, which is best practice when messages are being published or read
            // regularly.
            //
            // set the transport type to AmqpWebSockets so that the ServiceBusClient uses the port 443. 
            // If you use the default AmqpTcp, you will need to make sure that the ports 5671 and 5672 are open

            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };
            client = new ServiceBusClient(_configuration.AzureServiceBusPrimaryConnectionString, clientOptions);
            sender = client.CreateSender("transcription-service");
            _logger = logger;
        }

        public async Task SendMessageAsync(string subject, string jSonMessageContent, CancellationToken cancellationToken)
        {
            // create a batch 
            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

            var message = new ServiceBusMessage(jSonMessageContent);
            message.Subject = subject;
            message.ApplicationProperties.Add("Tenant", "TenantA");
            message.ApplicationProperties.Add("RequestTime", System.DateTime.Now);

            // RFC2045 Content-Type descriptor.
            message.ContentType = "application/json";
            
            if (!messageBatch.TryAddMessage(message))
            {
                _logger("The message is too large to fit in the batch.", true, true);
                // if it is too large for the batch
                throw new Exception($"The message is too large to fit in the batch.");
            }


            try
            {
                // Use the producer client to send the batch of messages to the Service Bus queue
                await sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"The message has been published to the queue.");
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }
        }

    }
}
