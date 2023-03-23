using System;

namespace BackgroundProcessing.Domain
{
    public class ProcessingRequest
    {
        public Guid? Id { get; set; }

        public string? InputValue { get; set; }

    }
}
