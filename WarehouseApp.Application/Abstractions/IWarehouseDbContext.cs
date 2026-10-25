using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Application.Abstractions;

/// <summary>
/// Интерфейс, представляющий контекст базы данных для работы со складскими сущностями.
/// </summary>
public interface IWarehouseDbContext
{
    /// <summary>
    /// Набор сущностей товаров.
    /// </summary>
    DbSet<Product> Products { get; }

    /// <summary>
    /// Набор сущностей категорий.
    /// </summary>
    DbSet<Category> Categories { get; }

    /// <summary>
    /// Набор сущностей единиц измерения.
    /// </summary>
    DbSet<UnitOfMeasure> UnitsOfMeasure { get; }

    /// <summary>
    /// Набор сущностей контрагентов.
    /// </summary>
    DbSet<Counterparty> Counterparties { get; }

    /// <summary>
    /// Набор сущностей складских документов.
    /// </summary>
    DbSet<WareDocument> Documents { get; }

    /// <summary>
    /// Набор сущностей позиций (строк) складских документов.
    /// </summary>
    DbSet<WareDocumentItem> DocumentItems { get; }


    /// <summary>
    /// Асинхронно сохраняет все изменения, отслеживаемые в контексте, в базе данных.
    /// </summary>
    /// <param name="ct">Токен отмены операции.</param>
    /// <returns>Количество записей, затронутых операцией сохранения.</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
