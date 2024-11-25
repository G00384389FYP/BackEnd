using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NixersDB;
using System.Linq;
using System.Threading.Tasks;

namespace NixersDB.Controllers
{
    [Route("api/[controller]")]
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

        [HttpPost("addUser")]
        public async Task<IActionResult> AddUser([FromBody] UserData userData)
        {
            _logger.LogInformation("Received a POST request to add a user.");

            if (!ModelState.IsValid)
            {
                _logger.LogError("Model state is invalid: {ModelStateErrors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }


            _logger.LogInformation("Name={Name}, Email={Email}, PhoneNumber={PhoneNumber}",
                userData.Name, userData.Email, userData.phone_number);


            _context.UserData.Add(userData);
            await _context.SaveChangesAsync();


            _logger.LogInformation("User created successfully with UserId={UserId}", userData.UserId);


            return Ok(new { Message = "User Profile received successfully", UserId = userData.UserId });
        }
    }
}
