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

    [HttpPut("{reviewId}")]
    public async Task<IActionResult> UpdateReview(Guid reviewId, [FromBody] ReviewData review)
    {
        if (review == null)
        {
            return BadRequest("Review data is null.");
        }

        try
        {
            var existingReview = await _context.Reviews.FindAsync(reviewId);
            if (existingReview == null)
            {
                return NotFound("Review not found.");
            }

            existingReview.WorkRating = review.WorkRating;
            existingReview.CustomerServiceRating = review.CustomerServiceRating;
            existingReview.PriceRating = review.PriceRating;
            existingReview.Comment = review.Comment;
            existingReview.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Review updated successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{reviewId}")]
    public async Task<IActionResult> DeleteReview(Guid reviewId)
    {
        try
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                return NotFound("Review not found.");
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return Ok("Review deleted successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
  

    [HttpGet]
    public async Task<IActionResult> GetReviews()
    {
        try
        {
            var reviews = await _context.Reviews
                .Join(
                    _context.UserData, 
                    review => review.ReviewerId,
                    user => user.UserId, 
                    (review, user) => new
                    {
                        review.ReviewId,
                        review.JobId,
                        review.InvoiceId,
                        review.WorkRating,
                        review.CustomerServiceRating,
                        review.PriceRating,
                        review.Comment,
                        review.CreatedAt,
                        review.UpdatedAt,
                        ReviewerName = user.Name 
                    }
                )
                .ToListAsync();

            return Ok(reviews);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("{reviewId}")]
    
    public async Task<IActionResult> GetReview()
    {
        try
        {
            var reviews = await _context.Reviews
                .Join(
                    _context.UserData, 
                    review => review.ReviewerId,
                    user => user.UserId, 
                    (review, user) => new
                    {
                        review.ReviewId,
                        review.JobId,
                        review.InvoiceId,
                        review.WorkRating,
                        review.CustomerServiceRating,
                        review.PriceRating,
                        review.Comment,
                        review.CreatedAt,
                        review.UpdatedAt,
                        ReviewerName = user.Name 
                    }
                )
                .ToListAsync();

            return Ok(reviews);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}