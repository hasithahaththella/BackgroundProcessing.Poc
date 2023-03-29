using BackgroundProcessing.Core;
using BackgroundProcessing.Data;
using BackgroundProcessing.Domain;
using BackgroundProcessing.ServiceBusQueue;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace BackgroundProcessing.Service
{
    

    public class BackgroundProcessingDataService
    {
        private readonly IServiceBusQueueService _serviceBusQueueService;
        private readonly IDbContextFactory<BackgroundProcessingContext> contextFactory;
        private readonly WriteLine logger;

        public BackgroundProcessingDataService(
            IServiceBusQueueService serviceBusQueueService,
            IDbContextFactory<BackgroundProcessingContext> contextFactory, 
            WriteLine logger)
        {
            this._serviceBusQueueService = serviceBusQueueService;
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

        public async Task<Guid> AddItemAsync(ProcessingRequest item, CancellationToken cancellationToken)
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


            try 
            {
                // Submit to Azure service bus queue
                await _serviceBusQueueService.SendMessageAsync(item.Id.Value.ToString(), 
                    JsonSerializer.Serialize(new DataStoreItem() { Id = item.Id.Value, InputValue = item.InputValue }),
                    cancellationToken);
            }
            catch (Exception ex) 
            {
                logger($"Adding new message to service bus queue has failed. {Environment.NewLine}{ex.Message}", true, true);
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
    }
}
