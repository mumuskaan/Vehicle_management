using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleParts.API.Data;
using VehicleParts.API.DTOs;

namespace VehicleParts.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return Unauthorized(new { message = "Invalid email or password" });

        if (user.PasswordHash != request.Password)
            return Unauthorized(new { message = "Invalid email or password" });

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        return Ok(new
        {
            userId = user.Id,
            customerId = customer != null ? customer.Id : 0,
            fullName = user.FullName,
            email = user.Email,
            role = user.Role
        });
    }
}