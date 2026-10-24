using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Application.Services.Records;

namespace WarehouseApp.Application.Services;

public interface IReportService
{
    Task<ReportSummary> GetSummaryAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<List<(DateTime Day, int Count)>> GetSalesByDaysAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<List<(string ProductName, int Quantity)>> GetLowStockAsync(int threshold, CancellationToken ct = default);
}
