 [HttpGet("report/top-spenders")]
    public async Task<IActionResult> GetTopSpenders()
    {
        var result = await _context.SalesInvoices
            .GroupBy(s => s.CustomerId)
            .Select(g => new
            {
                CustomerId = g.Key,
                TotalSpent = g.Sum(x => x.GrandTotal)
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(5)
            .ToListAsync();

        return Ok(result);
    }