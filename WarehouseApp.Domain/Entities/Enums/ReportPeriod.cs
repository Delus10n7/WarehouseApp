namespace WarehouseApp.Domain.Entities.Enums;

/// <summary>
/// Определяет временной период для формирования отчета.
/// </summary>
public enum ReportPeriod
{
    /// <summary>Не задан. Используется как значение по умолчанию (default).</summary>
    None = 0,

    /// <summary>Отчет за день.</summary>
    Day = 1,

    /// <summary>Отчет за неделю.</summary>
    Week = 2,

    /// <summary>Отчет за месяц.</summary>
    Month = 3,

    /// <summary>Отчет за год.</summary>
    Year = 4
}
