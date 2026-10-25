using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WarehouseApp.Api.Controllers;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Tests.Api;

/// <summary>
/// Тесты для <see cref="CounterpartiesController"/>. 
/// Проверяют возвращаемые HTTP-статусы и корректность вызовов репозитория.
/// </summary>
public class CounterpartiesControllerTests
{
    private readonly ICounterpartyRepository _repositoryMock;
    private readonly CounterpartiesController _controller;

    /// <summary>
    /// Инициализирует тестовый класс, создавая заглушку репозитория и экземпляр контроллера.
    /// </summary>
    public CounterpartiesControllerTests()
    {
        _repositoryMock = Substitute.For<ICounterpartyRepository>();
        _controller = new CounterpartiesController(_repositoryMock);
    }

    /// <summary>
    /// Проверяет, что метод GetAll возвращает статус 200 OK и список контрагентов.
    /// </summary>
    [Fact]
    public async Task GetAll_Should_Return_Ok_With_Data()
    {
        // Arrange
        var fakeData = new List<Counterparty> { new() { Id = 1, Name = "Тест" } };
        _repositoryMock.GetAllAsync(Arg.Any<CancellationToken>()).Returns(fakeData);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnData = okResult.Value.Should().BeAssignableTo<IEnumerable<Counterparty>>().Subject;

        returnData.Should().HaveCount(1);
        returnData.First().Name.Should().Be("Тест");
    }

    /// <summary>
    /// Проверяет, что метод Create вызывает AddAsync у репозитория и возвращает статус 200 OK.
    /// </summary>
    [Fact]
    public async Task Create_Should_Call_Repository_And_Return_Ok()
    {
        // Arrange
        var newCounterparty = new Counterparty { Name = "Новый" };

        // Act
        var result = await _controller.Create(newCounterparty);

        // Assert
        result.Should().BeOfType<OkResult>();
        await _repositoryMock.Received(1).AddAsync(newCounterparty, Arg.Any<CancellationToken>());
    }
}
