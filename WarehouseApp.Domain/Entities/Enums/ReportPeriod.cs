using System;
using System.Collections.Generic;
using System.Text;

namespace WarehouseApp.Domain.Entities.Enums;

/// <summary>
/// Определяет временной период для формирования отчета.
/// </summary>
public enum ReportPeriod
{
    Day,      /// Отчет за день.
    Week,     /// Отчет за неделю.
    Month,    /// Отчет за месяц.
    Year      /// Отчет за год.
}
