// Controllers/TradesmanController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NixersDB;
using NixersDB.Models;
using System;
using System.Threading.Tasks;

namespace NixersDB.Controllers
{
    [Route("tradies")]
    [ApiController]
    public class TradesmanController : ControllerBase
    {
        private readonly ILogger<TradesmanController> _logger;
        private readonly NixersDbContext _context;

        public TradesmanController(ILogger<TradesmanController> logger, NixersDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTradesmanProfile([FromBody] TradesmanProfileRequest request)
        {
            _logger.LogInformation("Received a POST request to create a tradesman profile from the frontend.");

            int userId = request.UserId;

            var user = await _context.UserData.FindAsync(userId);
            if (user == null)
            {
                _logger.LogError("User with ID {UserId} not found.", userId);
                return NotFound(new { Message = "User not found" });
            }

            var tradesmanProfile = new TradesmanData
            {
                UserId = userId,
                Trade = request.Trade,
                Location = request.Location,
                NumberOfJobsCompleted = 0,
                TradeBio = request.TradeBio,
                WorkDistance = request.WorkDistance,
                DateJoined = DateTime.Now
            };

            _context.TradesmanData.Add(tradesmanProfile);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Tradesman profile created successfully" });
        }
        
        [HttpPut("{userId}")]
        public async Task<IActionResult> IncerementJobsCompleted([FromBody] UserIdRequest request)
        {
            _logger.LogInformation("Received a PUT request to update a tradesman's completed jobs.");

            int userId = request.UserId;

            var tradesmanProfile = await _context.TradesmanData.FirstOrDefaultAsync(t => t.UserId == userId);
            if (tradesmanProfile == null)
            {
                _logger.LogError("Tradesman profile for UserId {UserId} not found.", userId);
                return NotFound(new { Message = "Tradesman profile not found" });
            }

            tradesmanProfile.NumberOfJobsCompleted += 1;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Tradesman profile updated successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> CheckTradesmanProfile([FromQuery] int userId)
        {
            _logger.LogInformation("Received a GET request to check if a tradesman profile exists.");

            var tradesmanProfile = await _context.TradesmanData.FirstOrDefaultAsync(t => t.UserId == userId);
            if (tradesmanProfile == null)
            {
                _logger.LogInformation("Tradesman profile for UserId {UserId} does not exist.", userId);
                return Ok(new { Exists = false, Message = "Tradesman profile does not exist" });
            }

            _logger.LogInformation("Tradesman profile for UserId {UserId} exists.", userId);

            var formattedDateJoined = tradesmanProfile.DateJoined.ToString("yyyy-MM-dd");

            return Ok(new
            {
                Exists = true,
                Message = "Tradesman profile exists",
                TradesmanProfile = new
                {
                    tradesmanProfile.UserId,
                    tradesmanProfile.Trade,
                    tradesmanProfile.Location,
                    tradesmanProfile.NumberOfJobsCompleted,
                    tradesmanProfile.TradeBio,
                    tradesmanProfile.WorkDistance,
                    DateJoined = formattedDateJoined
                }
            });
        }
    }

    public class TradesmanProfileRequest
    {
        public int UserId { get; set; }
        public string Trade { get; set; }
        public string Location { get; set; }
        public string TradeBio { get; set; }
        public double WorkDistance { get; set; }
    }
}