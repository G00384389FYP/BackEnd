using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NixersDB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly ILogger<JobsController> _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public JobsController(ILogger<JobsController> logger, CosmosClient cosmosClient)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
            _container = _cosmosClient.GetContainer("YourDatabaseName", "JobsContainer");
        }

        [HttpPost("createJob")]
        public async Task<IActionResult> CreateJob([FromBody] Job job)
        {
            job.Id = Guid.NewGuid().ToString();
            job.PostedDate = DateTime.UtcNow;

            await _container.CreateItemAsync(job, new PartitionKey(job.Id));

            return Ok(new { Message = "Job created successfully", JobId = job.Id });
        }

        [HttpGet("getJobs")]
        public async Task<IActionResult> GetJobs()
        {
            var query = _container.GetItemQueryIterator<Job>("SELECT * FROM c");
            var results = new List<Job>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return Ok(results);
        }
    }
}