using Microsoft.AspNetCore.Mvc;

namespace NixersDB.Controllers
{
    [Route("api/[controller]")]
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

        [HttpPost("createCustomerProfile")]
        public async Task<IActionResult> CreateCustomerProfile([FromBody] UserIdRequest request)
        {
            _logger.LogInformation("Received a POST request to create a customer profile from the frontend.");

            // handle userID sent from front end to an int
            int userId = request.UserId;

            // ensure userID posted exists
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
    }

    // handle request from front end, turn json info into int
    public class UserIdRequest
    {
        public int UserId { get; set; }
    }
}