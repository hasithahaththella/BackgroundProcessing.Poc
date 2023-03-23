using Azure.Core;
using BackgroundProcessing.Domain;
using BackgroundProcessing.Service;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundProcessing.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly ILogger<RequestController> _logger;
        private readonly BackgroundProcessingDataService _backgroundProcessingDataService;

        public RequestController(
            BackgroundProcessingDataService backgroundProcessingDataService,
            ILogger<RequestController> logger)
        {
            _backgroundProcessingDataService = backgroundProcessingDataService;
            _logger = logger;
        }

       
        [HttpGet(Name = "GetProcessedItems")]
        public async Task<ProcessedResponse> Get()
        {
            var r = await _backgroundProcessingDataService.GetProcessedItemsAsync();
            return r;
        }

        [HttpPost(Name = "SubmitItemForProcessing")]
        public async Task<Guid> Post(ProcessingRequest request)
        {
            return await _backgroundProcessingDataService.AddItemAsync(request);
        }

        [HttpDelete(Name = "RemoveProcessedItem")]
        public void Delete(Guid id)
        {
            //DataStore.Instance.RemoveItem(id);
        }
    }
}