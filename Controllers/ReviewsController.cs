using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Stripe;
using Stripe.Checkout;
using Microsoft.Azure.Cosmos;
using NixersDB;
using NixersDB.Models;


[Route("reviews")]
[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly NixersDbContext _context;
    private readonly IConfiguration _config;
    private readonly Container _cosmosContainer;
    private readonly CosmosClient _cosmosClient;
    public ReviewsController(NixersDbContext context, IConfiguration config, CosmosClient cosmosClient)
    {
        _context = context;
        _config = config;
        _cosmosClient = cosmosClient;
        _cosmosContainer = cosmosClient.GetContainer("nixers-cosmos-ne", "jobs-container");
    }

    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] ReviewData review)
    {
        if (review == null)
        {
            return BadRequest("Review data is null.");
        }

        try
        {
            review.ReviewId = Guid.NewGuid();
            review.CreatedAt = DateTime.UtcNow;
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            return Ok("Review created successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}