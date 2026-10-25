using System;
using System.Collections.Generic;
using System.Text;

namespace WarehouseApp.Application.Services.Records;

/// <summary>
/// Запись, содержащая общую сводку по продажам за период.
/// </summary>
/// <param name="Revenue">Общая выручка за период.</param>
/// <param name="Profit">Общая прибыль за период.</param>
/// <param name="TotalSales">Общее количество продаж (транзакций).</param>
/// <param name="AverageCheck">Средняя сумма одного чека (продажи).</param>
public record ReportSummary(
        decimal Revenue,
        decimal Profit,
        int TotalSales,
        decimal AverageCheck);
