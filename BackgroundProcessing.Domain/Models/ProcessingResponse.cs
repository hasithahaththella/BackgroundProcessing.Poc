using System;
using System.Collections.Generic;

namespace BackgroundProcessing.Domain
{
    public class ProcessedResponse
    {
        public List<ProcessedItem> ProcessedItems;
        public ProcessedResponse() 
        {
            ProcessedItems = new List<ProcessedItem>();
        }

        public int? NumberOfItemsQueued { get; set; }

        public int? NumberOfItemsInProcessing { get; set; }
    }

    public class ProcessedItem
    {
        public Guid Id { get; set; }

        public string? ProcessedValue { get; set; }
    }
}
