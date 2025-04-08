using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NixersDB;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace NixersDB.Controllers
{
    [Route("customers")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly NixersDbContext _context;

        public CustomerController(ILogger<CustomerController> logger, NixersDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomerProfile([FromBody] UserIdRequest request)
        {
            _logger.LogInformation("Received a POST request to create a customer profile from the frontend.");

            int userId = request.UserId;

            var user = await _context.UserData.FindAsync(userId);
            if (user == null)
            {
                _logger.LogError("User with ID {UserId} not found.", userId);
                return NotFound(new { Message = "User not found" });
            }

            var customerProfile = new CustomerData
            {
                UserId = userId,
                JobsPosted = 0,
                IsSuspended = false,
                DateAdded = DateTime.Now
            };

            _context.CustomerData.Add(customerProfile);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Customer profile created successfully" });
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> IncerementJobPosted([FromBody] UserIdRequest request)
        {
            _logger.LogInformation("Received a PUT request to update a customer profile.");

            int userId = request.UserId;

            var customerProfile = await _context.CustomerData.FindAsync(userId);
            if (customerProfile == null)
            {
                _logger.LogError("Customer profile for UserId {UserId} not found.", userId);
                return NotFound(new { Message = "Customer profile not found" });
            }

            customerProfile.JobsPosted += 1; 
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Customer profile updated successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> CheckCustomerProfile([FromQuery] int userId)
        {
            _logger.LogInformation("Received a GET request to check if a customer profile exists.");

            var customerProfile = await _context.CustomerData.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customerProfile == null)
            {
                _logger.LogInformation("Customer profile for UserId {UserId} does not exist.", userId);
                return Ok(new { Exists = false, Message = "Customer profile does not exist" });
            }

            _logger.LogInformation("Customer profile for UserId {UserId} exists.", userId);

            var formattedDateAdded = customerProfile.DateAdded.ToString("yyyy-MM-dd");

            return Ok(new
            {
                Exists = true,
                Message = "Customer profile exists",
                CustomerProfile = new
                {
                    customerProfile.UserId,
                    JobsPosted = customerProfile.JobsPosted.ToString(),
                    customerProfile.IsSuspended,
                    DateAdded = formattedDateAdded
                }
            });
        }     

        [HttpGet("job-applications")]
        public async Task<IActionResult> GetCustomerJobApplications([FromQuery] int userId)
        {
            _logger.LogInformation("Received a GET request to retrieve a customer's job applications.");

            var jobApplications = await _context.JobApplications
                .Where(ja => ja.CustomerId == userId)
                .Join(_context.UserData,
                      ja => ja.TradesmanId,
                      u => u.UserId,
                      (ja, u) => new
                      {
                          ja.Id,
                          ja.CreatedAt,
                          ja.CustomerId,
                          ja.JobId,
                          ja.Status,
                          ja.TradesmanId,
                          ja.UpdatedAt,
                          Tradesman = new
                          {
                              u.UserId,
                              u.Name,
                              u.Email 
                          }
                      })
                .ToListAsync();

            if (!jobApplications.Any())
            {
                _logger.LogInformation("No job applications found for UserId {UserId}.", userId);
                return NotFound(new { Message = "No job applications found" });
            }

            return Ok(jobApplications);
        }
    }

    public class UserIdRequest
    {
        public int UserId { get; set; }
    }
}