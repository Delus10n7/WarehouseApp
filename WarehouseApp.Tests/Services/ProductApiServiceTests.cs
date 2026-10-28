using System.Net;
using FluentAssertions;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Infrastructure.Services;

namespace WarehouseApp.Tests.Services;

/// <summary>
/// Тесты для <see cref="ProductApiService"/>. 
/// Проверяют HTTP-методы и обработку ошибок.
/// </summary>
public class ProductApiServiceTests
{
    [Fact]
    public async Task GetAllAsync_Should_Return_List_When_Response_Is_Success()
    {
        // Arrange
        var fakeJson = "[{\"id\":1,\"name\":\"Тестовый товар\"}]";
        var mockHandler = new MockHttpMessageHandler(fakeJson, HttpStatusCode.OK);
        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost/") };
        var service = new ProductApiService(httpClient);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Тестовый товар");
        mockHandler.LastRequestUri.Should().Be("http://localhost/api/products");
    }

    [Fact]
    public async Task AddAsync_Should_Throw_Exception_When_Server_Returns_Error()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler("", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost/") };
        var service = new ProductApiService(httpClient);
        var product = new Product { Name = "Тест" };

        // Act
        Func<Task> act = async () => await service.AddAsync(product);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
