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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob(string id)
        {
            _logger.LogInformation("Received a PUT request to update job with ID: {JobId}", id);

            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                    .WithParameter("@id", id);
                var iterator = _container.GetItemQueryIterator<JobData>(query);
                var job = await iterator.ReadNextAsync();

                if (job.Count == 0)
                {
                    _logger.LogWarning("Job with ID: {JobId} not found.", id);
                    return NotFound(new { Message = "Job not found" });
                }

                return Ok(job.First());
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Job with ID: {JobId} not found.", id);
                return NotFound(new { Message = "Job not found" });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetJobsByUserId(string userId)
        {
            _logger.LogInformation("Received a GET request to retrieve jobs for UserId: {UserId}", userId);

            var query = new QueryDefinition("SELECT * FROM c WHERE c.UserId = @userId")
                .WithParameter("@userId", userId);
            var iterator = _container.GetItemQueryIterator<JobData>(query);
            var results = new List<JobData>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            if (results.Count == 0)
            {
                _logger.LogWarning("No jobs found for UserId: {UserId}", userId);
                return NotFound(new { Message = "No jobs found" });
            }

            return Ok(results);
        }

        [HttpGet("tradesman/{userId}")]
        public async Task<IActionResult> GetAssignedJobsByUserId(string userId)
        {
            _logger.LogInformation("Received a GET request to retrieve assigned jobs for UserId: {UserId}", userId);

            var query = new QueryDefinition("SELECT * FROM c WHERE c.AssignedTradesman = @userId")
                .WithParameter("@userId", userId);
            var iterator = _container.GetItemQueryIterator<JobData>(query);
            var results = new List<JobData>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            if (results.Count == 0)
            {
                _logger.LogWarning("No jobs found for UserId: {UserId}", userId);
                return NotFound(new { Message = "No jobs found" });
            }

            return Ok(results);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutJobStatusComplete(string id)
        {
            _logger.LogInformation("Received a PUT request to update job status to complete for JobId: {JobId}", id);

            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
                    .WithParameter("@id", id);
                var iterator = _container.GetItemQueryIterator<JobData>(query);
                var jobDocument = await iterator.ReadNextAsync();

                if (!jobDocument.Any())
                {
                    _logger.LogWarning("Job with ID: {JobId} not found.", id);
                    return NotFound(new { Message = "Job not found" });
                }

                var job = jobDocument.First();
                job.JobStatus = "Complete";

                await _container.ReplaceItemAsync(job, job.Id, new PartitionKey(job.UserId));

                _logger.LogInformation("Job status updated to complete for JobId: {JobId}", id);
                return Ok(new { Message = "Job status updated to complete" });
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Job with ID: {JobId} not found.", id);
                return NotFound(new { Message = "Job not found" });
            }
        }
    }
}