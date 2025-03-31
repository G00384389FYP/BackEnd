using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NixersDB;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Stripe;

namespace NixersDB.Controllers
{
    [Route("users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly NixersDbContext _context;
        private readonly IConfiguration _config;
        public UserController(ILogger<UserController> logger, NixersDbContext context, IConfiguration config)
        {
            _logger = logger;
            _context = context;
            _config = config;
        }


        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] UserData userData)
        {

            if (!ModelState.IsValid)
            {
                _logger.LogError("Model state is invalid: {ModelStateErrors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            _context.UserData.Add(userData);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User Profile received successfully", UserId = userData.UserId });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserIdByEmail([FromQuery] string email)
        {
            try
            {
                var user = await _context.UserData.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return NotFound(new { Message = "Email not found" });
                }

                _logger.LogInformation("Email {Email} found with UserId={UserId}", email, user.UserId);
                return Ok(new { UserId = user.UserId });
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
                return StatusCode(500, "Database error occurred.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled Error: {ex.Message}");
                return StatusCode(500, "Something went wrong.");
            }
        }

        [HttpPost("stripe")]
        public async Task<IActionResult> ConnectStripeAccount([FromBody] string userEmail)
        {
            var accountOptions = new AccountCreateOptions
            {
                Type = "express",
                Country = "IE",
                Email = userEmail,
                Capabilities = new AccountCapabilitiesOptions
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
                    Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
                },
            };

            var accountService = new AccountService();
            var account = await accountService.CreateAsync(accountOptions);

            var returnUrl = _config["Stripe:ReturnUrlBase"] + "/stripe/complete";
            var refreshUrl = _config["Stripe:RefreshUrlBase"] + "/stripe/connect";

            var linkOptions = new AccountLinkCreateOptions
            {
                Account = account.Id,
                ReturnUrl = returnUrl,
                RefreshUrl = refreshUrl,
                Type = "account_onboarding"
            };


            var linkService = new AccountLinkService();
            var accountLink = await linkService.CreateAsync(linkOptions);



            return Ok(new { url = accountLink.Url });
        }

    }

    public class EmailRequest
    {
        public required string Email { get; set; }
    }
}
