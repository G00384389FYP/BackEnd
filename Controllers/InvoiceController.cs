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
    private readonly CosmosClient _cosmosClient;
    public InvoiceController(NixersDbContext context, IConfiguration config, CosmosClient cosmosClient)
    {
        _context = context;
        _config = config;
        _cosmosClient = cosmosClient;
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

    [HttpGet("{id}/paid")]
    public async Task<IActionResult> GetPaidInvoiceByCustomerId(string id)
    {
        var invoices = await _context.Invoices
            .Where(invoice => invoice.CustomerId == int.Parse(id) && invoice.Status == "Paid")
            .ToListAsync();

        if (!invoices.Any())
        {
            return Ok(new { Message = "No paid invoices found for the specified customer." });
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

    [HttpPost("pay/{invoiceId}")]
    public async Task<IActionResult> CreateCheckoutSession(Guid invoiceId)
    {
        var invoice = await _context.Invoices.FindAsync(invoiceId);
        if (invoice == null) return NotFound("Invoice not found");

        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

        var options = new SessionCreateOptions
        {
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
                        Description = $"Payment for Job with Invoice ID #{invoice.InvoiceId}"
                    }
                },
                Quantity = 1
            }
        },
            Mode = "payment",
            SuccessUrl = _config["Stripe:ReturnUrlBase"] + $"/payment/success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = _config["Stripe:ReturnUrlBase"] + $"/payment/cancel?invoiceId={invoiceId}",
            

        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);
        invoice.StripeCheckoutSessionId = session.Id;
        await _context.SaveChangesAsync();       

        

        return Ok(new { url = session.Url });
    }


    [HttpPut("confirm/{sessionId}")]
    public async Task<IActionResult> ConfirmPaymentAndUpdateInvoice(string sessionId)
    {
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

        
        var sessionService = new SessionService();
        var session = await sessionService.GetAsync(sessionId);
        Console.WriteLine(sessionId);
        Console.WriteLine(session.PaymentStatus);

        if (session.PaymentStatus != "paid")
            return BadRequest("Payment not completed.");

        await _context.SaveChangesAsync();

        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.StripeCheckoutSessionId == sessionId);

        if (invoice == null)
            return NotFound("Invoice not found.");

        invoice.Status = "Paid";
        invoice.PaymentDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();       
        

        return Ok("Paid.");
    }

    
   


    // Model for the request body for payments
    public class PayInvoiceRequest
    {
        public Guid InvoiceId { get; set; }
    }



}
