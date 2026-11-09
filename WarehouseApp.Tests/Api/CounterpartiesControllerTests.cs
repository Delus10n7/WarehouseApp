using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WarehouseApp.Api.Controllers;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Application.Dtos.CounterpartyDtos;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Tests.Api;

/// <summary>
/// Unit-тесты <see cref="CounterpartiesController"/>.
/// Проверяют HTTP-коды ответов и вызовы репозитория.
/// </summary>
public class CounterpartiesControllerTests
{
    /// <summary>Мок репозитория контрагентов.</summary>
    private readonly ICounterpartyRepository _repositoryMock;

    /// <summary>Тестируемый контроллер.</summary>
    private readonly CounterpartiesController _controller;

    /// <summary>Инициализирует мок и контроллер.</summary>
    public CounterpartiesControllerTests()
    {
        _repositoryMock = Substitute.For<ICounterpartyRepository>();
        _controller = new CounterpartiesController(_repositoryMock);
    }

    /// <summary>
    /// <c>GetAll</c> возвращает 200 OK со списком DTO.
    /// </summary>
    [Fact]
    public async Task GetAll_Returns_Ok_With_Dtos()
    {
        // Arrange
        _repositoryMock.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Counterparty>
            {
                new() { Id = 1, Name = "Тест" }
            });

        // Act
        var result = await _controller.GetAll(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dtos = okResult.Value.Should().BeAssignableTo<IEnumerable<CounterpartyDto>>().Subject.ToList();
        dtos.Should().HaveCount(1);
        dtos[0].Name.Should().Be("Тест");
    }

    /// <summary>
    /// <c>GetById</c> возвращает 404 Not Found для несуществующего Id.
    /// </summary>
    [Fact]
    public async Task GetById_Returns_NotFound_When_Not_Found()
    {
        _repositoryMock.GetByIdAsync(5, Arg.Any<CancellationToken>())
            .Returns((Counterparty?)null);

        var result = await _controller.GetById(5, CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// <c>GetById</c> возвращает 200 OK с DTO для существующего Id.
    /// </summary>
    [Fact]
    public async Task GetById_Returns_Ok_With_Dto()
    {
        _repositoryMock.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new Counterparty { Id = 1, Name = "Найдено" });

        var result = await _controller.GetById(1, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<CounterpartyDto>()
            .Which.Name.Should().Be("Найдено");
    }

    /// <summary>
    /// <c>Create</c> возвращает 400 Bad Request при пустом Name.
    /// </summary>
    [Fact]
    public async Task Create_Returns_BadRequest_When_Name_Empty()
    {
        var dto = new CounterpartyRequestDto { Name = "   " };

        var result = await _controller.Create(dto, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        await _repositoryMock.DidNotReceive()
            .AddAsync(Arg.Any<Counterparty>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// <c>Create</c> возвращает 201 Created и вызывает репозиторий.
    /// </summary>
    [Fact]
    public async Task Create_Returns_Created_When_Valid()
    {
        var dto = new CounterpartyRequestDto { Name = "Новый" };

        // Когда репозиторий вызывается, симулируем присвоение Id
        _repositoryMock.AddAsync(Arg.Any<Counterparty>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var entity = callInfo.Arg<Counterparty>();
                entity.Id = 42;
                return entity;
            });

        var result = await _controller.Create(dto, CancellationToken.None);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().BeOfType<CounterpartyDto>()
            .Which.Id.Should().Be(42);
    }

    /// <summary>
    /// <c>Update</c> возвращает 404 Not Found для несуществующего Id.
    /// </summary>
    [Fact]
    public async Task Update_Returns_NotFound_When_Not_Found()
    {
        _repositoryMock.GetByIdAsync(5, Arg.Any<CancellationToken>())
            .Returns((Counterparty?)null);

        var result = await _controller.Update(5, new CounterpartyRequestDto { Name = "X" }, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
        await _repositoryMock.DidNotReceive()
            .UpdateAsync(Arg.Any<Counterparty>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// <c>Update</c> возвращает 204 No Content и вызывает репозиторий.
    /// </summary>
    [Fact]
    public async Task Update_Returns_NoContent_When_Exists()
    {
        _repositoryMock.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new Counterparty { Id = 1, Name = "Старое" });

        var result = await _controller.Update(1, new CounterpartyRequestDto { Name = "Новое" }, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        await _repositoryMock.Received(1)
            .UpdateAsync(Arg.Is<Counterparty>(c => c.Id == 1 && c.Name == "Новое"), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// <c>Delete</c> возвращает 404 Not Found, если репозиторий вернул false.
    /// </summary>
    [Fact]
    public async Task Delete_Returns_NotFound_When_Not_Found()
    {
        _repositoryMock.DeleteAsync(5, Arg.Any<CancellationToken>()).Returns(false);

        var result = await _controller.Delete(5, CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// <c>Delete</c> возвращает 204 No Content, если репозиторий вернул true.
    /// </summary>
    [Fact]
    public async Task Delete_Returns_NoContent_When_Deleted()
    {
        _repositoryMock.DeleteAsync(1, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _controller.Delete(1, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }
}
