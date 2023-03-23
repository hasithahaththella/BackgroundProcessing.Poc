using BackgroundProcessing.Data;
using BackgroundProcessing.Domain;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BackgroundProcessing.Service
{
    public delegate void WriteLine(string text, bool highlight, bool isException);

    public class BackgroundProcessingDataService
    {
        private readonly IDbContextFactory<BackgroundProcessingContext> contextFactory;
        private readonly WriteLine logger;

        public BackgroundProcessingDataService(IDbContextFactory<BackgroundProcessingContext> contextFactory, WriteLine logger)
        {
            this.contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            this.logger = logger ?? throw new ArgumentNullException(nameof(contextFactory));
            CreateDatabaseIfNotExisit().Wait();
        }

        // TODO :: Find the best place to do this
        public async Task CreateDatabaseIfNotExisit()
        {
            using (var context = await contextFactory.CreateDbContextAsync())
            {
                try
                {
                    logger("Recreating a new database...", false, false);
                   // await context.Database.EnsureDeletedAsync();

                    await context.Database.EnsureCreatedAsync();
                }
                catch(Exception ex)
                {
                    logger($"Recreating a new database step has failed. {Environment.NewLine}{ex.Message}", true, true);
                    throw new ApplicationException(ex.Message, ex);
                }
            }
            
        }

        public async Task Start()
        {
            try
            {
                logger("Starting Background processing dataService", false, false);
                await CreateDatabaseIfNotExisit();
                logger("Successfully started Background processing data service", false, false);
            }
            catch (Exception ex) 
            {
                logger($"Starting Background processing data service step has failed. {Environment.NewLine}{ex.Message}", true, true);
                throw new ApplicationException(ex.Message, ex);
            }
        }

        public async Task<Guid> AddItemAsync(ProcessingRequest item)
        {
            logger("Adding new processing request..", false, false);

            item.Id = item.Id ?? Guid.NewGuid();

            try
            {
                using (var context = await contextFactory.CreateDbContextAsync())
                {
                    context.Add(new DataStoreItem
                    {
                        Id = item.Id.Value,
                        InputValue = item.InputValue,
                        State = State.Queued,
                        StringState = State.Queued.ToString()
                    });

                    await context.SaveChangesAsync();
                    logger($"New processing request : {item.Id} has been saved successfully..", false, false);
                }
            }
            catch (Exception ex)
            {
                logger($"Adding new processing request to database has failed. {Environment.NewLine}{ex.Message}", true, true);
                throw new ApplicationException(ex.Message, ex);
            }


            return item.Id.Value;
        }

        public async Task<ProcessedResponse> GetProcessedItemsAsync()
        {
            logger("Checking for processed requests..", false, false);

            using (var context = await contextFactory.CreateDbContextAsync())
            {
                var processedItems = context.DataStore.Where(d => d.State == State.Processed);

                var processingCount = context.DataStore.Count(d => d.State == State.Processing);
                var QueuededCount = context.DataStore.Count(d => d.State == State.Queued);

                logger($"Returning processed items : count {processedItems.Count()}, QueuededCount: {QueuededCount}, onProcessing:{processingCount}", false, false);

                var response = new ProcessedResponse
                {
                    NumberOfItemsInProcessing = processingCount,
                    NumberOfItemsQueued = QueuededCount
                };

                response.ProcessedItems = await processedItems.Select(r => new ProcessedItem
                {
                    Id = r.Id,
                    ProcessedValue = r.ProcessedValue
                }).ToListAsync();

                return response;
            }
        }

        //public Task<Guid> UpdateItem(ProcessedRequest item)
        //{
        //    logger("Adding new processing request..", false, false);

        //    try
        //    {
        //        using (var context = await contextFactory.CreateDbContextAsync())
        //        {
        //            context.Add(new DataStoreItem
        //            {
        //                Id = item.Id.Value,
        //                InputValue = item.InputValue,
        //                State = State.Queued
        //            });

        //            await context.SaveChangesAsync();
        //            logger($"New processing request : {item.Id} has been saved successfully..", false, false);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger($"Adding new processing request to database has failed. {Environment.NewLine}{ex.Message}", true, true);
        //        throw new ApplicationException(ex.Message, ex);
        //    }


        //    return item.Id.Value;
        //}

        //public void RemoveItem(Guid id)
        //{
        //    if (!items.ContainsKey(id))
        //        return;

        //    items.Remove(id, out _);
        //}

    }
}
