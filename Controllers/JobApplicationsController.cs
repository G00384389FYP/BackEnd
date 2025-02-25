using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NixersDB.Models;
using System;
using System.Threading.Tasks;

namespace NixersDB.Controllers
{
    [Route("jobs/{jobId}/applications")]
    [ApiController]
    public class JobApplicationsController : ControllerBase
    {
        private readonly ILogger<JobApplicationsController> _logger;
        private readonly NixersDbContext _context;

        public JobApplicationsController(ILogger<JobApplicationsController> logger, NixersDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateJobApplication(Guid jobId, [FromBody] JobApplications application)
        {
            _logger.LogInformation("Received a POST request to create a job application.");

            application.JobId = jobId;
            application.Id = Guid.NewGuid();
            application.Status = "pending";
            application.CreatedAt = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Job application created successfully", ApplicationId = application.Id });
        }
    }
}