using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Application.Abstractions;

/// <summary>
/// Интерфейс репозитория для управления сущностями <see cref="Counterparty"/> (контрагентами).
/// </summary>
public interface ICounterpartyRepository
{
    /// <summary>
    /// Асинхронно получает список всех контрагентов.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Список контрагентов.</returns>
    Task<List<Counterparty>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Асинхронно добавляет нового контрагента в базу данных.
    /// </summary>
    /// <param name="counterparty">Объект контрагента для добавления.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task AddAsync(Counterparty counterparty, CancellationToken ct = default);

    /// <summary>
    /// Асинхронно обновляет данные существующего контрагента.
    /// </summary>
    /// <param name="counterparty">Объект контрагента с обновленными данными.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task UpdateAsync(Counterparty counterparty, CancellationToken ct = default);

    /// <summary>
    /// Асинхронно удаляет контрагента из базы данных по его идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор контрагента.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task DeleteAsync(int id, CancellationToken ct = default);
}
