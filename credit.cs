[HttpGet("report/credit")]
    public async Task<IActionResult> GetCustomersWithCredit()
    {
        var customers = await _context.Customers
            .Include(c => c.User)
            .Where(c => c.CreditBalance > 0)
            .ToListAsync();

        return Ok(customers);
    }