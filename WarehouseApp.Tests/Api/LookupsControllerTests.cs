using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Api.Controllers;
using WarehouseApp.Application.Dtos.CategoryDtos;
using WarehouseApp.Application.Dtos.UnitOfMeasureDtos;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Infrastructure;

namespace WarehouseApp.Tests.Api;

/// <summary>
/// Unit-тесты <see cref="LookupsController"/>.
/// Использует реальный <see cref="WarehouseDbContext"/> на SQLite in-memory,
/// потому что контроллер работает с <c>IQueryable</c> и <c>ToListAsync</c>.
/// </summary>
public class LookupsControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly WarehouseDbContext _context;
    private readonly LookupsController _controller;

    /// <summary>Готовит SQLite in-memory и контроллер.</summary>
    public LookupsControllerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new WarehouseDbContext(options);
        _context.Database.EnsureCreated();

        _controller = new LookupsController(_context);
    }

    /// <summary>GetCategories на пустой БД возвращает 200 с пустым списком.</summary>
    [Fact]
    public async Task GetCategories_Returns_Empty_List_On_Empty_Db()
    {
        var result = await _controller.GetCategories(CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<CategoryDto>>().Subject.Should().BeEmpty();
    }

    /// <summary>CreateCategory возвращает 400 при пустом Name.</summary>
    [Fact]
    public async Task CreateCategory_Returns_BadRequest_When_Name_Empty()
    {
        var result = await _controller.CreateCategory(
            new CategoryCreateDto { Name = "" }, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>CreateCategory сохраняет в БД и возвращает DTO с Id.</summary>
    [Fact]
    public async Task CreateCategory_Persists_And_Returns_Dto()
    {
        var result = await _controller.CreateCategory(
            new CategoryCreateDto { Name = "Новая" }, CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<CategoryDto>().Subject;
        dto.Id.Should().BeGreaterThan(0);
        dto.Name.Should().Be("Новая");

        // Проверяем, что действительно сохранилось
        var saved = await _context.Categories.AsNoTracking().ToListAsync();
        saved.Should().HaveCount(1);
    }

    /// <summary>GetUnits на пустой БД возвращает 200 с пустым списком.</summary>
    [Fact]
    public async Task GetUnits_Returns_Empty_List_On_Empty_Db()
    {
        var result = await _controller.GetUnits(CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<UnitOfMeasureDto>>().Subject.Should().BeEmpty();
    }

    /// <summary>CreateUnit возвращает 400 при пустом Name.</summary>
    [Fact]
    public async Task CreateUnit_Returns_BadRequest_When_Name_Empty()
    {
        var result = await _controller.CreateUnit(
            new UnitOfMeasureCreateDto { Name = "   " }, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>CreateUnit сохраняет в БД и возвращает DTO.</summary>
    [Fact]
    public async Task CreateUnit_Persists_And_Returns_Dto()
    {
        var result = await _controller.CreateUnit(
            new UnitOfMeasureCreateDto { Name = "кг" }, CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<UnitOfMeasureDto>().Subject;
        dto.Id.Should().BeGreaterThan(0);
        dto.Name.Should().Be("кг");
    }

    /// <summary>Освобождает ресурсы.</summary>
    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
