using Microsoft.AspNetCore.Mvc;
using VehicleParts.API.Data;
using VehicleParts.API.Models;

namespace VehicleParts.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly AppDbContext _context;

    public VehiclesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVehicle(int id, Vehicle updatedVehicle)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);

        if (vehicle == null)
            return NotFound(new { message = "Vehicle not found" });

        vehicle.VehicleNumber = updatedVehicle.VehicleNumber;
        vehicle.Brand = updatedVehicle.Brand;
        vehicle.Model = updatedVehicle.Model;
        vehicle.Year = updatedVehicle.Year;

        await _context.SaveChangesAsync();

        return Ok(vehicle);
    }
    [HttpPost]
    public async Task<IActionResult> AddVehicle(Vehicle vehicle)
    {
        var customer = await _context.Customers.FindAsync(vehicle.CustomerId);

        if (customer == null)
            return NotFound(new { message = "Customer not found" });

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        return Ok(vehicle);
    }
}