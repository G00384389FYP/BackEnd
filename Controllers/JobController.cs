using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using NixersDB.Models;

namespace NixersDB.Controllers
{
    [Route("jobs")]
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
            _container = _cosmosClient.GetContainer("nixers-cosmos-ne", "jobs-container");
        }

        [HttpPost]

        public async Task<IActionResult> CreateJob([FromBody] JobData job)
        {
            _logger.LogInformation("Received Job: {JobData}", JsonSerializer.Serialize(job));

            job.Id = Guid.NewGuid().ToString();
            var response = await _container.CreateItemAsync(job, new PartitionKey(job.UserId));
            return Ok(response.Resource);

        }

        [HttpGet]
        public async Task<IActionResult> GetJobs()
        {
            var query = _container.GetItemQueryIterator<JobData>("SELECT * FROM c");
            var results = new List<JobData>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return Ok(results);
        }
    }
}