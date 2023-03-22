namespace BackgroundProcessing.Poc.Models
{
    public class DataStoreItem
    {
        public Guid Id { get; set; }

        public State State { get; set; }

        public string CorelationId { get; set; }

        public string InputValue { get; set; }

        public string ProcessedValue { get; set; }

    }

    public enum State
    {
        Queued,
        Processing,
        Processed,
        Sent
    }

}
