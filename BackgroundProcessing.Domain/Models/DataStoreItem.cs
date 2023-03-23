using System;

namespace BackgroundProcessing.Domain
{
    public class DataStoreItem
    {
        public Guid Id { get; set; }

        public State State { get; set; }

        public string? StringState { get; set; } // TO support CosmosDb partitioning, Improve this

        public string? CorelationId { get; set; }

        public string? InputValue { get; set; }

        public string? ProcessedValue { get; set; }

    }

    public enum State
    {
        Queued,
        Processing,
        Processed,
        Sent
    }

}
