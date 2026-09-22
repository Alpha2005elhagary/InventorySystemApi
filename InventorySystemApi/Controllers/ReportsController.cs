using InventorySystemApi.Data;
using InventorySystemApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardKpiDto>> GetDashboardMetrics()
    {
        var products = await _context.Products.Include(p => p.Category).AsNoTracking().ToListAsync();
        var suppliersCount = await _context.Suppliers.CountAsync();

        var totalProducts = products.Count;
        var totalStock = products.Sum(p => p.Quantity);
        var totalInventoryValue = products.Sum(p => p.Price * p.Quantity);
        var lowStockCount = products.Count(p => p.Quantity <= p.MinStock && p.Quantity > 0);
        var outOfStockCount = products.Count(p => p.Quantity <= 0);

        // 1. Last 6 Months Revenue vs Purchases
        var now = DateTime.UtcNow;
        var sixMonthsAgo = new DateTime(now.Year, now.Month, 1).AddMonths(-5);

        var transactionsLast6Months = await _context.StockTransactions
            .Include(t => t.Product)
            .Where(t => t.Date >= sixMonthsAgo)
            .AsNoTracking()
            .ToListAsync();

        var monthlyLabels = new List<string>();
        var monthlySales = new List<decimal>();
        var monthlyPurchases = new List<decimal>();

        for (int i = 5; i >= 0; i--)
        {
            var target = now.AddMonths(-i);
            var mStart = new DateTime(target.Year, target.Month, 1);
            var mEnd = mStart.AddMonths(1);

            monthlyLabels.Add(mStart.ToString("MMM yyyy"));

            var mTx = transactionsLast6Months.Where(t => t.Date >= mStart && t.Date < mEnd).ToList();
            var sales = mTx.Where(t => t.Type == "Stock Out").Sum(t => t.Quantity * (t.Product?.Price ?? 0m));
            var purchases = mTx.Where(t => t.Type == "Stock In").Sum(t => t.Quantity * (t.Product?.Price ?? 0m));

            monthlySales.Add(sales);
            monthlyPurchases.Add(purchases);
        }

        // 2. Last 7 Days Daily Movements
        var sevenDaysAgo = now.Date.AddDays(-6);
        var transactionsLast7Days = await _context.StockTransactions
            .Where(t => t.Date >= sevenDaysAgo)
            .AsNoTracking()
            .ToListAsync();

        var weeklyLabels = new List<string>();
        var weeklyStockIn = new List<int>();
        var weeklyStockOut = new List<int>();

        for (int i = 6; i >= 0; i--)
        {
            var day = now.Date.AddDays(-i);
            var nextDay = day.AddDays(1);

            weeklyLabels.Add(day.ToString("ddd d"));

            var dayTx = transactionsLast7Days.Where(t => t.Date >= day && t.Date < nextDay).ToList();
            weeklyStockIn.Add(dayTx.Where(t => t.Type == "Stock In").Sum(t => t.Quantity));
            weeklyStockOut.Add(dayTx.Where(t => t.Type == "Stock Out").Sum(t => t.Quantity));
        }

        // 3. Category Breakdown
        var categoryBreakdown = products
            .GroupBy(p => p.Category?.Name ?? "Uncategorized")
            .Select(g => new CategoryBreakdownDto
            {
                CategoryName = g.Key,
                ProductCount = g.Count()
            })
            .OrderByDescending(c => c.ProductCount)
            .ToList();

        return Ok(new DashboardKpiDto
        {
            TotalProducts = totalProducts,
            TotalStock = totalStock,
            TotalInventoryValue = totalInventoryValue,
            LowStockCount = lowStockCount,
            OutOfStockCount = outOfStockCount,
            SuppliersCount = suppliersCount,
            MonthlyLabels = monthlyLabels,
            MonthlySalesData = monthlySales,
            MonthlyPurchasesData = monthlyPurchases,
            WeeklyLabels = weeklyLabels,
            WeeklyStockIn = weeklyStockIn,
            WeeklyStockOut = weeklyStockOut,
            CategoryBreakdown = categoryBreakdown
        });
    }

    [HttpGet("monthly")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<MonthlyReportDto>>> GetMonthlyReport([FromQuery] int months = 12)
    {
        var now = DateTime.UtcNow;
        var startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-(months - 1));

        var transactions = await _context.StockTransactions
            .Include(t => t.Product)
            .Where(t => t.Date >= startDate)
            .AsNoTracking()
            .ToListAsync();

        var reports = new List<MonthlyReportDto>();

        for (int i = months - 1; i >= 0; i--)
        {
            var target = now.AddMonths(-i);
            var mStart = new DateTime(target.Year, target.Month, 1);
            var mEnd = mStart.AddMonths(1);

            var mTx = transactions.Where(t => t.Date >= mStart && t.Date < mEnd).ToList();
            var sales = mTx.Where(t => t.Type == "Stock Out").Sum(t => t.Quantity * (t.Product?.Price ?? 0m));
            var purchases = mTx.Where(t => t.Type == "Stock In").Sum(t => t.Quantity * (t.Product?.Price ?? 0m));
            var tax = sales * 0.14m; // 14% VAT
            var net = sales - purchases - tax;

            reports.Add(new MonthlyReportDto
            {
                Month = mStart.ToString("MMMM yyyy"),
                SalesRevenue = sales,
                PurchasesCost = purchases,
                TaxCollected = tax,
                NetBalance = net
            });
        }

        return Ok(reports);
    }

    [HttpGet("database-overview")]
    [Authorize]
    public async Task<ActionResult<object>> GetDatabaseOverview()
    {
        var categoriesCount = await _context.Categories.CountAsync();
        var suppliersCount = await _context.Suppliers.CountAsync();
        var productsCount = await _context.Products.CountAsync();
        var stockTransactionsCount = await _context.StockTransactions.CountAsync();
        var usersCount = await _context.Users.CountAsync();
        var totalStockUnits = await _context.Products.SumAsync(p => p.Quantity);
        var totalInventoryValue = await _context.Products.SumAsync(p => p.Price * p.Quantity);

        return Ok(new
        {
            databaseName = "InventorySystemDb",
            serverStatus = "Connected & Online",
            timestamp = DateTime.UtcNow,
            entities = new
            {
                categories = categoriesCount,
                suppliers = suppliersCount,
                products = productsCount,
                stockTransactions = stockTransactionsCount,
                users = usersCount
            },
            inventorySummary = new
            {
                totalStockUnits,
                totalInventoryValue
            }
        });
    }
}
