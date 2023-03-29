namespace BackgroundProcessing.ServiceBusQueue
{
    public interface IServiceBusQueueService
    {
        Task SendMessageAsync(string subject, string jSonMessageContent, CancellationToken cancellationToken);

    }
}