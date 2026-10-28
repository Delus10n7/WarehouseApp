using System.Net;
using FluentAssertions;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Infrastructure.Services;

namespace WarehouseApp.Tests.Services;

/// <summary>
/// Тесты для <see cref="LookupApiService"/>. 
/// Проверяют запросы справочников.
/// </summary>
public class LookupApiServiceTests
{
    [Fact]
    public async Task GetCategoriesAsync_Should_Return_List()
    {
        // Arrange
        var fakeJson = "[{\"id\":1,\"name\":\"Категория 1\"}]";
        var mockHandler = new MockHttpMessageHandler(fakeJson, HttpStatusCode.OK);
        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost/") };
        var service = new LookupApiService(httpClient);

        // Act
        var result = await service.GetCategoriesAsync();

        // Assert
        result.Should().HaveCount(1);
        mockHandler.LastRequestUri.Should().Be("http://localhost/api/lookups/categories");
    }

    [Fact]
    public async Task AddCategoryAsync_Should_Return_Created_Entity()
    {
        // Arrange
        var fakeJson = "{\"id\":5,\"name\":\"Новая категория\"}";
        var mockHandler = new MockHttpMessageHandler(fakeJson, HttpStatusCode.OK);
        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost/") };
        var service = new LookupApiService(httpClient);
        var category = new Category { Name = "Новая категория" };

        // Act
        var result = await service.AddCategoryAsync(category);

        // Assert
        result.Id.Should().Be(5);
        result.Name.Should().Be("Новая категория");
    }
}
