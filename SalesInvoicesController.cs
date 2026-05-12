using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleParts.API.Data;
using VehicleParts.API.Models;
using VehicleParts.API.Services;

namespace VehicleParts.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesInvoicesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;
    private readonly IConfiguration _config;

    public SalesInvoicesController(
        AppDbContext context,
        EmailService emailService,
        IConfiguration config)
    {
        _context = context;
        _emailService = emailService;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> GetSalesInvoices()
    {
        var invoices = await _context.SalesInvoices
            .Include(s => s.Customer)
            .ThenInclude(c => c!.User)
            .ToListAsync();

        return Ok(invoices);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSalesInvoice(int id)
    {
        var invoice = await _context.SalesInvoices
            .Include(s => s.Customer)
            .ThenInclude(c => c!.User)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (invoice == null)
            return NotFound(new { message = "Sales invoice not found" });

        var items = await _context.SalesInvoiceItems
            .Include(i => i.Part)
            .Where(i => i.SalesInvoiceId == id)
            .ToListAsync();

        return Ok(new
        {
            invoice,
            items
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateSalesInvoice(CreateSalesInvoiceDto request)
    {
        var customer = await _context.Customers
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId);

        if (customer == null)
            return NotFound(new { message = "Customer not found" });

        if (request.Items == null || request.Items.Count == 0)
            return BadRequest(new { message = "At least one item is required" });

        decimal subTotal = 0;

        var invoice = new SalesInvoice
        {
            CustomerId = request.CustomerId,
            SaleDate = DateTime.UtcNow,
            IsCredit = request.IsCredit,
            IsPaid = !request.IsCredit
        };

        _context.SalesInvoices.Add(invoice);
        await _context.SaveChangesAsync();

        var invoiceItems = new List<SalesInvoiceItem>();

        foreach (var item in request.Items)
        {
            var part = await _context.Parts.FindAsync(item.PartId);

            if (part == null)
                return NotFound(new { message = $"Part with ID {item.PartId} not found" });

            if (part.StockQuantity < item.Quantity)
                return BadRequest(new { message = $"Not enough stock for {part.PartName}" });

            var totalPrice = part.SellingPrice * item.Quantity;

            part.StockQuantity -= item.Quantity;

            await SendLowStockEmailIfNeeded(part);

            var salesItem = new SalesInvoiceItem
            {
                SalesInvoiceId = invoice.Id,
                PartId = part.Id,
                Quantity = item.Quantity,
                UnitPrice = part.SellingPrice,
                TotalPrice = totalPrice
            };

            invoiceItems.Add(salesItem);
            subTotal += totalPrice;
        }

        decimal discountAmount = 0;

        if (subTotal > 5000)
        {
            discountAmount = subTotal * 0.10m;
        }

        invoice.SubTotal = subTotal;
        invoice.DiscountAmount = discountAmount;
        invoice.GrandTotal = subTotal - discountAmount;

        if (request.IsCredit)
        {
            customer.CreditBalance += invoice.GrandTotal;
            customer.LastCreditPaymentDate = DateTime.UtcNow;
        }

        _context.SalesInvoiceItems.AddRange(invoiceItems);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            invoice,
            items = invoiceItems
        });
    }

    private async Task SendLowStockEmailIfNeeded(Part part)
    {
        if (part.StockQuantity >= part.MinimumStockLevel)
            return;

        var adminEmail = _config["EmailSettings:SenderEmail"];

        if (string.IsNullOrWhiteSpace(adminEmail))
            return;

        var subject = "Low Stock Alert - Vehicle Parts System";

        var body =
$@"Dear Admin,

The following part is below minimum stock level after a sale:

Part Name: {part.PartName}
Part Code: {part.PartCode}
Current Stock: {part.StockQuantity}
Minimum Stock Level: {part.MinimumStockLevel}

Please restock this item soon.

Vehicle Parts System";

        await _emailService.SendEmailAsync(adminEmail, subject, body);
    }

    public class CreateSalesInvoiceDto
    {
        public int CustomerId { get; set; }
        public bool IsCredit { get; set; }
        public List<CreateSalesInvoiceItemDto> Items { get; set; } = new();
    }

    public class CreateSalesInvoiceItemDto
    {
        public int PartId { get; set; }
        public int Quantity { get; set; }
    }
}