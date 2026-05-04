using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleParts.API.Data;
using VehicleParts.API.Models;

namespace VehicleParts.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendorsController : ControllerBase
{
    private readonly AppDbContext _context;

    public VendorsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/vendors
    [HttpGet]
    public async Task<IActionResult> GetAllVendors()
    {
        var vendors = await _context.Vendors.ToListAsync();
        return Ok(vendors);
    }

    // GET: api/vendors/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVendorById(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);

        if (vendor == null)
            return NotFound(new { message = "Vendor not found" });

        return Ok(vendor);
    }

    // POST: api/vendors
    [HttpPost]
    public async Task<IActionResult> CreateVendor(Vendor vendor)
    {
        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetVendorById), new { id = vendor.Id }, vendor);
    }

    // PUT: api/vendors/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVendor(int id, Vendor updatedVendor)
    {
        var vendor = await _context.Vendors.FindAsync(id);

        if (vendor == null)
            return NotFound(new { message = "Vendor not found" });

        vendor.VendorName = updatedVendor.VendorName;
        vendor.ContactPerson = updatedVendor.ContactPerson;
        vendor.PhoneNumber = updatedVendor.PhoneNumber;
        vendor.Email = updatedVendor.Email;
        vendor.Address = updatedVendor.Address;

        await _context.SaveChangesAsync();

        return Ok(vendor);
    }

    // DELETE: api/vendors/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVendor(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);

        if (vendor == null)
            return NotFound(new { message = "Vendor not found" });

        _context.Vendors.Remove(vendor);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Vendor deleted successfully" });
    }
}