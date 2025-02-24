using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NixersDB;
using System.Linq;
using System.Threading.Tasks;

namespace NixersDB.Controllers
{
    [Route("users/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly NixersDbContext _context;

        public UserController(ILogger<UserController> logger, NixersDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddUser([FromBody] UserData userData)
        {
            // _logger.LogInformation("Received a POST request to add a user.");

            if (!ModelState.IsValid)
            {
                _logger.LogError("Model state is invalid: {ModelStateErrors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            // _logger.LogInformation("Name={Name}, Email={Email}, PhoneNumber={PhoneNumber}",
            //     userData.Name, userData.Email, userData.phone_number);

            _context.UserData.Add(userData);
            await _context.SaveChangesAsync();

            // _logger.LogInformation("User created successfully with UserId={UserId}", userData.UserId);

            return Ok(new { Message = "User Profile received successfully", UserId = userData.UserId });
        }

        [HttpPost("checkEmail")]
        public async Task<IActionResult> CheckEmail([FromBody] EmailRequest request)
        {
            // _logger.LogInformation("Received a POST request to check if an email exists.");
            
            var user = await _context.UserData.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                // _logger.LogInformation("Email {Email} not found.", request.Email);
                return NotFound(new { Message = "Email not found" }); // have the 404 handled by the front end as no email is found then redirect to create user, may need to change to a 200 with a message to be tider but this is fine for now
            }

            _logger.LogInformation("Email {Email} found with UserId={UserId}", request.Email, user.UserId);
            return Ok(new { UserId = user.UserId });
        }
    }

    public class EmailRequest
    {
        public string Email { get; set; }
    }
}
