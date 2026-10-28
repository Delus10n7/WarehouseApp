using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WarehouseApp.Api.Controllers;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Tests.Api;

/// <summary>
/// Тесты для <see cref="ProductsController"/>. 
/// Проверяют корректность возвращаемых HTTP-статусов и вызовов репозитория.
/// </summary>
public class ProductsControllerTests
{
    private readonly IProductRepository _repositoryMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _repositoryMock = Substitute.For<IProductRepository>();
        _controller = new ProductsController(_repositoryMock);
    }

    /// <summary>
    /// Проверяет, что метод GetAll возвращает 200 OK и список товаров.
    /// </summary>
    [Fact]
    public async Task GetAll_Should_Return_Ok_With_Data()
    {
        // Arrange
        var fakeData = new List<Product> { new() { Id = 1, Name = "Товар 1" } };
        _repositoryMock.GetAllWithDetailsAsync(Arg.Any<CancellationToken>()).Returns(fakeData);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnData = okResult.Value.Should().BeAssignableTo<IEnumerable<Product>>().Subject;
        returnData.Should().HaveCount(1);
    }

    /// <summary>
    /// Проверяет, что метод Create вызывает AddAsync и возвращает 200 OK.
    /// </summary>
    [Fact]
    public async Task Create_Should_Call_Repository_And_Return_Ok()
    {
        // Arrange
        var newProduct = new Product { Name = "Новый товар" };

        // Act
        var result = await _controller.Create(newProduct);

        // Assert
        result.Should().BeOfType<OkResult>();
        await _repositoryMock.Received(1).AddAsync(newProduct, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Проверяет, что метод Update вызывает UpdateAsync и возвращает 204 NoContent.
    /// </summary>
    [Fact]
    public async Task Update_Should_Call_Repository_And_Return_NoContent()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Обновленный" };

        // Act
        var result = await _controller.Update(1, product);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        await _repositoryMock.Received(1).UpdateAsync(product, Arg.Any<CancellationToken>());
    }
}
