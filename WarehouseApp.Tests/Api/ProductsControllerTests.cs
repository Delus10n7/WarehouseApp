using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WarehouseApp.Api.Controllers;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Application.Dtos.ProductDtos;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Tests.Api;

/// <summary>
/// Unit-тесты <see cref="ProductsController"/>.
/// </summary>
public class ProductsControllerTests
{
    private readonly IProductRepository _repositoryMock;
    private readonly ProductsController _controller;

    /// <summary>Инициализирует мок и контроллер.</summary>
    public ProductsControllerTests()
    {
        _repositoryMock = Substitute.For<IProductRepository>();
        _controller = new ProductsController(_repositoryMock);
    }

    /// <summary>GetAll возвращает 200 с DTO (включая CategoryName).</summary>
    [Fact]
    public async Task GetAll_Returns_Ok_With_Dtos()
    {
        _repositoryMock.GetAllWithDetailsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Product>
            {
                new()
                {
                    Id = 1,
                    Name = "Товар",
                    Article = "T-1",
                    Category = new Category { Name = "Кат" }
                }
            });

        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<ProductDto>>().Subject.ToList();
        dtos.Should().HaveCount(1);
        dtos[0].CategoryName.Should().Be("Кат");
    }

    /// <summary>GetById возвращает 404 при отсутствии записи.</summary>
    [Fact]
    public async Task GetById_Returns_NotFound_When_Not_Found()
    {
        _repositoryMock.GetByIdAsync(5, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var result = await _controller.GetById(5, CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>Create возвращает 400 при пустых Name/Article.</summary>
    [Theory]
    [InlineData("", "A-1")]
    [InlineData("Товар", "")]
    [InlineData("  ", "  ")]
    public async Task Create_Returns_BadRequest_When_Required_Fields_Empty(string name, string article)
    {
        var dto = new ProductRequestDto { Name = name, Article = article, CategoryId = 1, UnitOfMeasureId = 1 };

        var result = await _controller.Create(dto, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        await _repositoryMock.DidNotReceive()
            .AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    /// <summary>Create возвращает 201 Created с подгруженными навигациями.</summary>
    [Fact]
    public async Task Create_Returns_Created_With_Dto()
    {
        var dto = new ProductRequestDto { Name = "New", Article = "A-1", CategoryId = 1, UnitOfMeasureId = 1 };

        // При добавлении симулируем присвоение Id
        _repositoryMock.AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => { var p = callInfo.Arg<Product>(); p.Id = 42; return p; });

        // GetById после Create — для догрузки навигаций
        _repositoryMock.GetByIdAsync(42, Arg.Any<CancellationToken>())
            .Returns(new Product { Id = 42, Name = "New", Article = "A-1" });

        var result = await _controller.Create(dto, CancellationToken.None);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.Value.Should().BeOfType<ProductDto>().Which.Id.Should().Be(42);
    }

    /// <summary>Update возвращает 404 при отсутствии записи.</summary>
    [Fact]
    public async Task Update_Returns_NotFound_When_Not_Found()
    {
        _repositoryMock.GetByIdAsync(99, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var result = await _controller.Update(99, new ProductRequestDto { Name = "X", Article = "X" }, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        await _repositoryMock.DidNotReceive()
            .UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    /// <summary>Update возвращает 204 при существующей записи.</summary>
    [Fact]
    public async Task Update_Returns_NoContent_When_Exists()
    {
        _repositoryMock.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new Product { Id = 1, Name = "Старое" });

        var result = await _controller.Update(1,
            new ProductRequestDto { Name = "Новое", Article = "N-1", CategoryId = 2, UnitOfMeasureId = 3 },
            CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        await _repositoryMock.Received(1)
            .UpdateAsync(Arg.Is<Product>(p => p.Id == 1 && p.Name == "Новое" && p.Article == "N-1"),
                          Arg.Any<CancellationToken>());
    }

    /// <summary>Delete возвращает 404, если репозиторий вернул false.</summary>
    [Fact]
    public async Task Delete_Returns_NotFound_When_Not_Found()
    {
        _repositoryMock.DeleteAsync(5, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _controller.Delete(5, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>Delete возвращает 204 при успехе.</summary>
    [Fact]
    public async Task Delete_Returns_NoContent_When_Deleted()
    {
        _repositoryMock.DeleteAsync(1, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _controller.Delete(1, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }
}
