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
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Список контрагентов.</returns>
    Task<List<Counterparty>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Получает контрагента по его идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор контрагента.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Объект контрагента.</returns>
    Task<Counterparty?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Создает нового контрагента.
    /// </summary>
    /// <param name="counterparty">Объект контрагента для добавления.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task AddAsync(Counterparty counterparty, CancellationToken ct = default);

    /// <summary>
    /// Обновляет данные существующего контрагента.
    /// </summary>
    /// <param name="counterparty">Объект контрагента с обновленными данными.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task UpdateAsync(Counterparty counterparty, CancellationToken ct = default);

    /// <summary>
    /// Удаляет контрагента по идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор контрагента.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task DeleteAsync(int id, CancellationToken ct = default);
}
