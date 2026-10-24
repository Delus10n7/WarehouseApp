using System;
using System.Collections.Generic;
using System.Text;

namespace WarehouseApp.Application.Services.Records;

public record ReportSummary(
        decimal Revenue,
        decimal Profit,
        int TotalSales,
        decimal AverageCheck);
