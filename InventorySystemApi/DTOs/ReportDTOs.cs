namespace InventorySystemApi.DTOs;

public class DashboardKpiDto
{
    public int TotalProducts { get; set; }
    public int TotalStock { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public int SuppliersCount { get; set; }

    public List<string> MonthlyLabels { get; set; } = new();
    public List<decimal> MonthlySalesData { get; set; } = new();
    public List<decimal> MonthlyPurchasesData { get; set; } = new();

    public List<string> WeeklyLabels { get; set; } = new();
    public List<int> WeeklyStockIn { get; set; } = new();
    public List<int> WeeklyStockOut { get; set; } = new();

    public List<CategoryBreakdownDto> CategoryBreakdown { get; set; } = new();
}

public class CategoryBreakdownDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}

public class MonthlyReportDto
{
    public string Month { get; set; } = string.Empty;
    public decimal SalesRevenue { get; set; }
    public decimal PurchasesCost { get; set; }
    public decimal TaxCollected { get; set; }
    public decimal NetBalance { get; set; }
}
