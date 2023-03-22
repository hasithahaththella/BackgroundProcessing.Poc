using BackgroundProcessing.Poc.Models;
using System.Collections.Concurrent;

namespace BackgroundProcessing.Poc
{
    public sealed class DataStore
    {
        private static readonly Lazy<DataStore> lazy =
            new Lazy<DataStore>(() => new DataStore());

        public static DataStore Instance { get { return lazy.Value; } }

        private DataStore()
        {
            items = new ConcurrentDictionary<Guid, DataStoreItem>();
        }

        private ConcurrentDictionary<Guid, DataStoreItem> items;

        public IEnumerable<DataStoreItem> Items => DataStore.Instance.items.Values.AsEnumerable();

        public Guid AddItem(ProcessingRequest item)
        {
            Guid storeId = (item?.Id == null || item.Id == Guid.Empty || items.ContainsKey(item.Id.Value)) 
                ? Guid.NewGuid()
                : item.Id.Value;
            

            items.TryAdd(storeId, new DataStoreItem { Id = storeId, InputValue = item.InputValue, State = State.Queued });

            return storeId;
        }

        public void UpdateItem(ProcessedRequest item)
        {
            if (string.IsNullOrWhiteSpace(item.CorelationId))
            {
                return;
            }

            var itemToUpdate = items.Values.FirstOrDefault(i => i.State == State.Processing && i.CorelationId == item.CorelationId);

            if (itemToUpdate == null)
            {
                return;
            }

            itemToUpdate.ProcessedValue = item.OutPutValue;
            itemToUpdate.State = State.Processed;
        }

        public void RemoveItem(Guid id)
        {
            if (!items.ContainsKey(id))
                return;

            items.Remove(id, out _);
        }
    }
}