using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Application.Services.Records;

namespace WarehouseApp.Application.Services;

/// <summary>
/// Сервис для формирования аналитических отчетов по работе склада.
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Асинхронно получает общую сводку по продажам за указанный период.
    /// </summary>
    /// <param name="from">Дата начала периода.</param>
    /// <param name="to">Дата окончания периода.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Объект <see cref="ReportSummary"/> с общей информацией (выручка, прибыль и т.д.).</returns>
    Task<ReportSummary> GetSummaryAsync(DateTime from, DateTime to, CancellationToken ct = default);

    /// <summary>
    /// Асинхронно получает статистику продаж с разбивкой по дням за указанный период.
    /// </summary>
    /// <param name="from">Дата начала периода.</param>
    /// <param name="to">Дата окончания периода.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Список кортежей, содержащих дату и количество продаж в этот день.</returns>
    Task<List<(DateTime Day, int Count)>> GetSalesByDaysAsync(DateTime from, DateTime to, CancellationToken ct = default);

    /// <summary>
    /// Асинхронно получает список товаров, остаток которых на складе ниже указанного порога.
    /// </summary>
    /// <param name="threshold">Минимально допустимый остаток товара.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Список кортежей, содержащих название товара и его текущий остаток.</returns>
    Task<List<(string ProductName, int Quantity)>> GetLowStockAsync(int threshold, CancellationToken ct = default);
}
