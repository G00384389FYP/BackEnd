using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NixersDB; 

namespace NixersDB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            // _context = context; // Commented out to pause database interaction
            _logger = logger;
        }

        [HttpPost("addUser")]
        public async Task<IActionResult> AddUser([FromBody] UserData userData)
        {
            _logger.LogInformation("Received a POST request to add a user.");
            
            if (!ModelState.IsValid)
            {
                _logger.LogError("Model state is invalid: {ModelStateErrors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            // Log the received user data
            _logger.LogInformation("Name={Name}, Email={Email}, PhoneNumber={PhoneNumber}",
               userData.Name, userData.Email, userData.PhoneNumber);

            // Comment out the database save operation for now
            // _context.UserData.Add(userData);
            // await _context.SaveChangesAsync();

            return Ok(new { Message = "User Profile received successfully" });
        }
    }
}
