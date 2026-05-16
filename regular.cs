[HttpGet("report/regular")]
    public async Task<IActionResult> GetRegularCustomers()
    {
        var result = await _context.SalesInvoices
            .GroupBy(s => s.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                TotalPurchases = g.Count()
            })
            .OrderByDescending(x => x.TotalPurchases)
            .ToListAsync();

        return Ok(result);
    }