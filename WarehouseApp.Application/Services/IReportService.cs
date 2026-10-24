using System;
using System.Collections.Generic;
using System.Text;

namespace WarehouseApp.Application.Services
{
    public record ReportSummary(
        decimal Revenue,
        decimal Profit,
        int TotalSales,
        decimal AverageCheck);

    public interface IReportService
    {
        Task<ReportSummary> GetSummaryAsync(DateTime from, DateTime to, CancellationToken ct = default);
        Task<List<(DateTime Day, int Count)>> GetSalesByDaysAsync(DateTime from, DateTime to, CancellationToken ct = default);
        Task<List<(string ProductName, int Quantity)>> GetLowStockAsync(int threshold, CancellationToken ct = default);
    }
}
