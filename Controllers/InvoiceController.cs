using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Stripe;
using NixersDB;
using NixersDB.Models;

[Route("invoices")]
[ApiController]
public class InvoiceController : ControllerBase
{
    private readonly NixersDbContext _context;
    private readonly IConfiguration _config;

    public InvoiceController(NixersDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
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

        return Ok(invoice);
    }

    // [HttpPost("stripe")]
    // public async Task<IActionResult> CreatePaymentIntent([FromBody] int amount, [FromServices] IConfiguration configuration)
    // {
    //     // Retrieve the Stripe API key from appsettings.json
    //     StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];

    //     var options = new PaymentIntentCreateOptions
    //     {
    //         Amount = amount,
    //         Currency = "eur", 
    //         AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
    //         {
    //             Enabled = true,
    //         },
    //     };

    //     var service = new PaymentIntentService();
    //     var paymentIntent = await service.CreateAsync(options);

    //     return Ok(new { clientSecret = paymentIntent.ClientSecret });
    // }

    [HttpPost("payment-intent")]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] int amount)
    {
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"]; // make sure this is set

        var options = new PaymentIntentCreateOptions
        {
            Amount = amount, // in cents, e.g. €10 = 1000
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
