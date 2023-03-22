using BackgroundProcessing.Poc.Models;
using Microsoft.AspNetCore.Mvc;

namespace BackgroundProcessing.Poc.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RequestController : ControllerBase
    {
        private readonly ILogger<RequestController> _logger;

        public RequestController(ILogger<RequestController> logger)
        {
            _logger = logger;
        }

       
        [HttpGet(Name = "GetProcessedItems")]
        public IEnumerable<ProcessingResponse> Get()
        {
            return DataStore.Instance.Items.Where(i => i.State == State.Processed).Select( r => new ProcessingResponse { Id = r.Id, ProcessedValue = r.ProcessedValue}).AsEnumerable();  
        }

        [HttpPost(Name = "SubmitItemForProcessing")]
        public Guid Post(ProcessingRequest request)
        {
            return DataStore.Instance.AddItem(request);
        }

        [HttpDelete(Name = "RemoveProcessedItem")]
        public void Delete(Guid id)
        {
            DataStore.Instance.RemoveItem(id);
        }
    }
}