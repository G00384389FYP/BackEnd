using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Stripe;
using Stripe.Checkout;
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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInvoiceByCustomerId(string id)
    {
        var invoices = await _context.Invoices
            .Where(invoice => invoice.CustomerId == int.Parse(id))
            .ToListAsync();

        if (!invoices.Any())
        {
            return Ok(new { Message = "No invoices found for the specified customer." });
        }

        return Ok(invoices);
    }



    [HttpPost("{id}")]
    public async Task<IActionResult> CreateInvoice([FromBody] Invoice invoice)
    {
        invoice.InvoiceId = Guid.NewGuid();
        invoice.CreatedAt = DateTime.UtcNow;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.IssuedDate = DateTime.UtcNow;
        invoice.Status = "Pending";

        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
        var options = new PaymentIntentCreateOptions
        {
            Amount = (int)(invoice.Amount * 100), 
            Currency = invoice.Currency.ToLower(),
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true,
            },
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);

        invoice.StripePaymentIntentId = intent.Id;

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();

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

    [HttpPost("pay/{id}")]
    public async Task<IActionResult> CreateCheckoutSession(Guid id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null) return NotFound("Invoice not found");

        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

        // Create a chckout session
        var options = new SessionCreateOptions
        {
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                SetupFutureUsage = "off_session"
            },
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = invoice.Currency.ToLower(),
                        UnitAmount = (long)(invoice.Amount * 100), 
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Invoice Payment",
                            Description = $"Payment for Job #{invoice.JobId}"
                        },
                    },
                    Quantity = 1,
                }
            },
            Mode = "payment",
            SuccessUrl = $"{_config["Stripe:ReturnUrlBase"]}/payment/success?invoiceId={id}",
            CancelUrl = $"{_config["Stripe:ReturnUrlBase"]}/payment/cancel?invoiceId={id}"
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        return Ok(new { url = session.Url });
    }


    // Model for the request body for payments
    public class PayInvoiceRequest
    {
        public Guid InvoiceId { get; set; }
    }



}
