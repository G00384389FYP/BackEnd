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

        private readonly IBlobStorageService _blobStorageService;


        public JobsController(ILogger<JobsController> logger, CosmosClient cosmosClient, IBlobStorageService blobStorageService)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
            _container = _cosmosClient.GetContainer("nixers-cosmos-ne", "jobs-container");
            _blobStorageService = blobStorageService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateJob([FromBody] JobData job)
        {
            _logger.LogInformation("Received Job: {JobData}", JsonSerializer.Serialize(job));

            job.Id = Guid.NewGuid().ToString();
            var response = await _container.CreateItemAsync(job, new PartitionKey(job.UserId));
            return Ok(response.Resource);
        }

        [HttpPost("image")]
        [ApiExplorerSettings(IgnoreApi = false)] 
        [Consumes("multipart/form-data")] 
        public async Task<IActionResult> UploadJobImages([FromForm] UploadJobImageRequest file)
        {
            if (file == null || file.File.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var blobName = $"{Guid.NewGuid()}-{file.FileName}";

            using (var stream = file.File.OpenReadStream())
            {
                await _blobStorageService.UploadBlobAsync("job-media", blobName, stream);
            }

            var blobUrl = $"https://nixersstorage.blob.core.windows.net/job-media/{blobName}";
            return Ok(new { Message = "File uploaded successfully", Url = blobUrl });
        }


        

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(string id)
        {
            _logger.LogInformation("Received a DELETE request for JobId: {JobId}", id);

            try
            {
                // Query to check if the job exists and retrieve its UserId ( i shouldve made the id the partition key but i was lazy)
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
                _logger.LogInformation("Job with ID: {JobId} found. Proceeding to delete.", id);

                var response = await _container.DeleteItemAsync<JobData>(id, new PartitionKey(job.UserId));
                _logger.LogInformation("Job with ID: {JobId} successfully deleted.", id);

                return Ok(new { Message = "Job deleted successfully", DeletedJob = response.Resource });
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Job with ID: {JobId} not found during deletion.", id);
                return NotFound(new { Message = "Job not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the job with ID: {JobId}", id);
                return StatusCode(500, new { Message = "An error occurred while deleting the job", Error = ex.Message });
            }
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

            var query = new QueryDefinition("SELECT * FROM c WHERE c.UserId = @userId AND c.JobStatus = @jobStatus")
                .WithParameter("@userId", userId)
                .WithParameter("@jobStatus", "Open");
            var iterator = _container.GetItemQueryIterator<JobData>(query);
            var results = new List<JobData>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            if (results.Count == 0)
            {
                _logger.LogWarning("No jobs found for UserId: {UserId} with status 'Open'", userId);
                return Ok(new List<JobData>());
            }

            return Ok(results);
        }

        [HttpGet("user/{userId}/completed")]
        public async Task<IActionResult> GetCompletedJobsByUserId(string userId)
        {
            _logger.LogInformation("Received a GET request to retrieve completed jobs for UserId: {UserId}", userId);

            var query = new QueryDefinition("SELECT * FROM c WHERE c.UserId = @userId AND c.JobStatus = @jobStatus")
                .WithParameter("@userId", userId)
                .WithParameter("@jobStatus", "Complete");
            var iterator = _container.GetItemQueryIterator<JobData>(query);
            var results = new List<JobData>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            if (results.Count == 0)
            {
                _logger.LogWarning("No completed jobs found for UserId: {UserId}", userId);
                return Ok(new List<JobData>());
            }

            return Ok(results);
        }



        // Going to stop calling this but keeping it here cuz ill probably need it later reminder to remove if not ****************
        [HttpGet("tradies/{userId}")]
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
                // return Ok(new { Message = "No jobs found", Jobs = new List<JobData>() });
                return Ok(new List<JobData>());
            }

            return Ok(results);
        }

        [HttpGet("tradies/{userId}/closed")]
        public async Task<IActionResult> GetInProgressAssignedJobsByUserId(string userId)
        {
            _logger.LogInformation("Received a GET request to retrieve assigned jobs for UserId: {UserId}", userId);

            var query = new QueryDefinition("SELECT * FROM c WHERE c.AssignedTradesman = @userId AND c.JobStatus = @jobStatus")
                .WithParameter("@userId", userId)
                .WithParameter("@jobStatus", "closed");
            var iterator = _container.GetItemQueryIterator<JobData>(query);
            var results = new List<JobData>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            if (results.Count == 0)
            {
                _logger.LogWarning("No jobs found for UserId: {UserId} with status 'closed'", userId);
                return Ok(new List<JobData>());
            }

            return Ok(results);
        }

        [HttpGet("tradesman/{userId}/completed")]
        public async Task<IActionResult> GetCompletedAssignedJobsByUserId(string userId)
        {
            _logger.LogInformation("Received a GET request to retrieve assigned jobs for UserId: {UserId}", userId);

            var query = new QueryDefinition("SELECT * FROM c WHERE c.AssignedTradesman = @userId AND c.JobStatus = @jobStatus")
                .WithParameter("@userId", userId)
                .WithParameter("@jobStatus", "Complete");
            var iterator = _container.GetItemQueryIterator<JobData>(query);
            var results = new List<JobData>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            if (results.Count == 0)
            {
                _logger.LogWarning("No jobs found for UserId: {UserId} with status 'Complete'", userId);
                return Ok(new List<JobData>());
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