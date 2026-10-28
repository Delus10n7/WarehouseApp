using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Application.Services;

/// <summary>
/// Интерфейс сервиса для работы с API контрагентов.
/// </summary>
public interface ICounterpartyApiService
{
    /// <summary>
    /// Получает список всех контрагентов.
    /// </summary>
    Task<List<Counterparty>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Получает контрагента по его идентификатору.
    /// </summary>
    Task<Counterparty?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Создает нового контрагента.
    /// </summary>
    Task AddAsync(Counterparty counterparty, CancellationToken ct = default);

    /// <summary>
    /// Обновляет данные существующего контрагента.
    /// </summary>
    Task UpdateAsync(Counterparty counterparty, CancellationToken ct = default);

    /// <summary>
    /// Удаляет контрагента по идентификатору.
    /// </summary>
    Task DeleteAsync(int id, CancellationToken ct = default);
}
