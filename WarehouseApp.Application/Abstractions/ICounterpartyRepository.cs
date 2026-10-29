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
    Task<IReadOnlyList<Counterparty>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Асинхронно получает контрагента по идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор контрагента.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Объект контрагента или <c>null</c>, если не найден.</returns>
    Task<Counterparty?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Асинхронно добавляет нового контрагента в базу данных.
    /// </summary>
    /// <param name="counterparty">Объект контрагента для добавления.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Созданный объект контрагента.</returns>
    Task<Counterparty> AddAsync(Counterparty counterparty, CancellationToken ct = default);

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
    /// <returns>true если нашли и удалили объект</returns>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
