using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using NixersDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace NixersDB.Controllers
{
    [Route("jobs/{jobId}/applications")]
    [ApiController]
    public class JobApplicationsController : ControllerBase
    {
        private readonly ILogger<JobApplicationsController> _logger;
        private readonly NixersDbContext _context;
        private readonly Container _jobContainer;

        public JobApplicationsController(ILogger<JobApplicationsController> logger, NixersDbContext context, CosmosClient cosmosClient)
        {
            _logger = logger;
            _context = context;
            _jobContainer = cosmosClient.GetContainer("nixers-cosmos-ne", "jobs-container");
        }

        [HttpPost]
        public async Task<IActionResult> CreateJobApplication(Guid jobId, [FromBody] JobApplications application)
        {
            _logger.LogInformation("Received a POST request to create a job application.");

            application.JobId = jobId;
            application.Id = Guid.NewGuid();
            application.CustomerId = application.CustomerId;
            application.Status = "pending";
            application.CreatedAt = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Job application created successfully", ApplicationId = application.Id });
        }

        [HttpPut("{applicationId}")]
        public async Task<IActionResult> UpdateJobApplication(Guid applicationId, [FromBody] JobApplications application)
        {
            _logger.LogInformation("Received a PUT request to update a job application.");
            _logger.LogInformation("Job Application: {JobApplication}", JsonSerializer.Serialize(application));
            _logger.LogInformation("Job Application tradesman ID: {TradesmanId}", application.TradesmanId);

            var existingApplication = await _context.JobApplications.FindAsync(applicationId);
            if (existingApplication == null)
            {
                return NotFound(new { Message = "Job application not found" });
            }

            existingApplication.Status = application.Status;
            // existingApplication.TradesmanId = application.TradesmanId; 
            existingApplication.UpdatedAt = DateTime.UtcNow;

            _context.JobApplications.Update(existingApplication);
            await _context.SaveChangesAsync();

            // _logger.LogInformation("Updated job application: {JobApplication}", JsonSerializer.Serialize(existingApplication));
            // _logger.LogInformation("Updated job application tradesman ID: {TradesmanId}", existingApplication.TradesmanId);

            var jobQuery = new QueryDefinition("SELECT * FROM c WHERE c.id = @jobId")
                .WithParameter("@jobId", existingApplication.JobId.ToString());
            var jobIterator = _jobContainer.GetItemQueryIterator<JobData>(jobQuery);
            var jobDocument = await jobIterator.ReadNextAsync();

            if (jobDocument.Any())
            {
                var job = jobDocument.First();
                // job.AssignedTradesman = existingApplication.TradesmanId.ToString(); 
                job.JobStatus = "closed"; 
                job.AssignedTradesman = existingApplication.TradesmanId.ToString(); 
                   
                await _jobContainer.ReplaceItemAsync(job, job.Id);
            }
            else
            {
                return NotFound(new { Message = "Job document not found in Cosmos DB" });
            }

            return Ok(new { Message = "Job application and job document updated successfully" });
        }

        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetJobApplicationsForCustomer(int customerId)
        {
            _logger.LogInformation("Received a GET request to retrieve job applications for customer ID: {CustomerId}", customerId);

            var jobApplications = await _context.JobApplications.ToListAsync();

            if (!jobApplications.Any())
            {
                return NotFound(new { Message = "No job applications found." });
            }

            var jobIds = jobApplications.Select(app => app.JobId.ToString()).ToList();
            var query = new QueryDefinition("SELECT * FROM c WHERE ARRAY_CONTAINS(@jobIds, c.id)")
                .WithParameter("@jobIds", jobIds);
            var iterator = _jobContainer.GetItemQueryIterator<JobData>(query);
            var jobDataList = new List<JobData>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                jobDataList.AddRange(response.ToList());
            }

            // Filter job applications based on CustomerId in job data
            var filteredApplications = jobApplications
                .Where(app => jobDataList.Any(job => job.Id == app.JobId.ToString() && int.Parse(job.UserId) == customerId))
                .ToList();

            if (!filteredApplications.Any())
            {
                return NotFound(new { Message = "No job applications found for the customer." });
            }

            // Link job applications with job data
            var result = filteredApplications.Select(app => new
            {
                Application = app,
                Job = jobDataList.FirstOrDefault(job => job.Id == app.JobId.ToString())
            });

            return Ok(result);
        }
    }
}
