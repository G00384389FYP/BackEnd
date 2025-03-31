using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Stripe;
using Microsoft.Azure.Cosmos;
using NixersDB;
using NixersDB.Models;

[Route("invoices")]
[ApiController]
public class InvoiceController : ControllerBase
{
    private readonly NixersDbContext _context;
    private readonly IConfiguration _config;
    private readonly Container _cosmosContainer;
    public InvoiceController(NixersDbContext context, IConfiguration config, CosmosClient cosmosClient)
    {
        _context = context;
        _config = config;
            _cosmosContainer = cosmosClient.GetContainer("nixers-cosmos-ne", "jobs-container");
        }

    [HttpPost("{id}")]
    public async Task<IActionResult> CreateInvoice([FromBody] Invoice invoice)
    {
        invoice.InvoiceId = Guid.NewGuid();
        invoice.CreatedAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.IssuedDate = DateTime.UtcNow;
        invoice.Status = "Pending";

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Update job status in Cosmos DB to be invoiced
        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @jobId")
            .WithParameter("@jobId", invoice.JobId);
        var iterator = _cosmosContainer.GetItemQueryIterator<JobData>(query);

        if (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            var job = response.FirstOrDefault();

            if (job != null)
            {
                job.JobStatus = "Invoiced";
                await _cosmosContainer.ReplaceItemAsync(job, job.Id, new PartitionKey(job.UserId));
            }
        }

        return Ok(invoice);
    }
   

    [HttpPost("payment-intent")]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] int amount)
    {
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"]; 

        var options = new PaymentIntentCreateOptions
        {
            Amount = amount, // ****** in cents ******
            Currency = "eur",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);

        return Ok(new { clientSecret = intent.ClientSecret });
    }

}
