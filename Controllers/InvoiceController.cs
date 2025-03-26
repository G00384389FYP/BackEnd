using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using NixersDB;
using NixersDB.Models;

[Route("invoices")]
[ApiController]
public class InvoiceController : ControllerBase
{
    private readonly NixersDbContext _context;
    public InvoiceController(NixersDbContext context)
    {
        _context = context;
    }

    [HttpPost]
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
}
