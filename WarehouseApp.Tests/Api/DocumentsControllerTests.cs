using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WarehouseApp.Api.Controllers;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Application.Dtos.CommonDtos;
using WarehouseApp.Application.Dtos.WareDocumentDtos;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Domain.Entities.Enums;

namespace WarehouseApp.Tests.Api;

/// <summary>
/// Unit-тесты <see cref="DocumentsController"/>.
/// </summary>
public class DocumentsControllerTests
{
    private readonly IWareDocumentRepository _repositoryMock;
    private readonly DocumentsController _controller;

    public DocumentsControllerTests()
    {
        _repositoryMock = Substitute.For<IWareDocumentRepository>();
        _controller = new DocumentsController(_repositoryMock);
    }

    /// <summary>
    /// Проверяет, что GetAll возвращает 200 OK со списком DTO.
    /// </summary>
    [Fact]
    public async Task GetAll_Returns_Ok_With_Dtos()
    {
        _repositoryMock.GetAllWithDetailsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<WareDocument> { new() { Id = 1, Number = "1" } });

        var result = await _controller.GetAll(CancellationToken.None);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<WareDocumentDto>>().Subject.Should().HaveCount(1);
    }

    /// <summary>
    /// Проверяет, что GetById возвращает 404 для несуществующего документа.
    /// </summary>
    [Fact]
    public async Task GetById_Returns_NotFound_When_Not_Found()
    {
        _repositoryMock.GetByIdAsync(5, Arg.Any<CancellationToken>())
            .Returns((WareDocument?)null);

        var result = await _controller.GetById(5, CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// Проверяет, что Create возвращает 400, если DTO не заполнен.
    /// </summary>
    [Fact]
    public async Task Create_Returns_BadRequest_When_Invalid()
    {
        var dto = new WareDocumentCreateDto { Number = "", CounterpartyId = 0 };

        var result = await _controller.Create(dto, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    /// Проверяет, что Create возвращает 201 Created при успешном сохранении.
    /// </summary>
    [Fact]
    public async Task Create_Returns_Created_When_Valid()
    {
        var dto = new WareDocumentCreateDto
        {
            Number = "DOC-1",
            CounterpartyId = 1,
            Items = new List<WareDocumentItemCreateDto> { new() { ProductId = 1, Quantity = 5, Price = 100 } }
        };

        _repositoryMock.AddAsync(Arg.Any<WareDocument>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => { var doc = callInfo.Arg<WareDocument>(); doc.Id = 10; return doc; });

        _repositoryMock.GetByIdAsync(10, Arg.Any<CancellationToken>())
            .Returns(new WareDocument { Id = 10, Number = "DOC-1" });

        var result = await _controller.Create(dto, CancellationToken.None);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    /// <summary>
    /// Проверяет, что Create ловит исключение нехватки товара и возвращает 400 с ProblemDetails.
    /// </summary>
    [Fact]
    public async Task Create_Returns_BadRequest_With_ProblemDetails_On_Stock_Shortage()
    {
        var dto = new WareDocumentCreateDto
        {
            Number = "DOC-1",
            CounterpartyId = 1,
            Items = new List<WareDocumentItemCreateDto> { new() { ProductId = 1, Quantity = 10, Price = 100 } }
        };

        _repositoryMock.AddAsync(Arg.Any<WareDocument>(), Arg.Any<CancellationToken>())
            .Returns<Task<WareDocument>>(_ => throw new InvalidOperationException("Недостаточно товара"));

        var result = await _controller.Create(dto, CancellationToken.None);

        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var problem = badRequest.Value.Should().BeOfType<Microsoft.AspNetCore.Mvc.ProblemDetails>().Subject;
        problem.Title.Should().Be("Недостаточно товара");
        problem.Detail.Should().Contain("Недостаточно товара");
    }

    /// <summary>
    /// Проверяет, что Update возвращает 404 для несуществующего документа.
    /// </summary>
    [Fact]
    public async Task Update_Returns_NotFound_When_Not_Found()
    {
        _repositoryMock.GetByIdAsync(5, Arg.Any<CancellationToken>())
            .Returns((WareDocument?)null);

        var result = await _controller.Update(5, new WareDocumentCreateDto(), CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// Проверяет, что Delete возвращает 204 при успешном удалении.
    /// </summary>
    [Fact]
    public async Task Delete_Returns_NoContent_When_Deleted()
    {
        _repositoryMock.DeleteAsync(1, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _controller.Delete(1, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }
}
