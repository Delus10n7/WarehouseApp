using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Application.Abstractions;

/// <summary>
/// Интерфейс репозитория для управления сущностями <see cref="WareDocument"/> (складскими документами).
/// </summary>
public interface IWareDocumentRepository
{
    /// <summary>
    /// Асинхронно получает список всех документов с подгруженными контрагентами и товарами.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Список документов.</returns>
    Task<IReadOnlyList<WareDocument>> GetAllWithDetailsAsync(CancellationToken ct = default);

    /// <summary>
    /// Асинхронно получает документ по идентификатору с подгруженными контрагентами и товарами.
    /// </summary>
    /// <param name="id">Уникальный идентификатор документа.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Объект документа или <c>null</c>, если не найден.</returns>
    Task<WareDocument?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Асинхронно добавляет новый документ и обновляет остатки товаров.
    /// </summary>
    /// <param name="document">Объект документа для добавления.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Созданный объект документа (с присвоенным Id).</returns>
    Task<WareDocument> AddAsync(WareDocument document, CancellationToken ct = default);

    /// <summary>
    /// Асинхронно обновляет существующий документ и корректирует остатки товаров.
    /// </summary>
    /// <param name="document">Объект документа с обновлёнными данными.</param>
    /// <param name="ct">Токен отмены операции.</param>
    Task UpdateAsync(WareDocument document, CancellationToken ct = default);

    /// <summary>
    /// Асинхронно удаляет документ и отменяет его влияние на остатки товаров.
    /// </summary>
    /// <param name="id">Уникальный идентификатор документа.</param>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns><c>true</c>, если запись была найдена и удалена; иначе <c>false</c>.</returns>
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
