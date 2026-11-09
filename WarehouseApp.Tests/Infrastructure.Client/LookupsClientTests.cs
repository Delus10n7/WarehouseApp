using System.Net;
using FluentAssertions;
using WarehouseApp.Application.Dtos.CategoryDtos;
using WarehouseApp.Application.Dtos.UnitOfMeasureDtos;
using WarehouseApp.Infrastructure.Client.Generated;
using WarehouseApp.Tests.TestHelpers;

namespace WarehouseApp.Tests.Infrastructure.Client;

/// <summary>
/// Unit-тесты NSwag-сгенерированного <see cref="LookupsClient"/>.
/// </summary>
public class LookupsClientTests
{
    /// <summary>GetCategoriesAsync возвращает список категорий.</summary>
    [Fact]
    public async Task GetCategoriesAsync_Returns_List()
    {
        const string json = "[{\"id\":1,\"name\":\"Кат1\"},{\"id\":2,\"name\":\"Кат2\"}]";
        var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var client = new LookupsClient(http);

        var result = await client.GetCategoriesAsync();

        result.Should().HaveCount(2);
        result.Select(c => c.Name).Should().Equal("Кат1", "Кат2");
        handler.LastRequest!.RequestUri!.ToString().Should().Be("http://localhost/api/Lookups/categories");
    }

    /// <summary>CreateCategoryAsync отправляет POST и возвращает созданный объект.</summary>
    [Fact]
    public async Task CreateCategoryAsync_Returns_Created()
    {
        const string json = "{\"id\":5,\"name\":\"Новая\"}";
        var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var client = new LookupsClient(http);

        var result = await client.CreateCategoryAsync(new CategoryCreateDto { Name = "Новая" });

        result.Id.Should().Be(5);
        result.Name.Should().Be("Новая");
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
    }

    /// <summary>GetUnitsAsync возвращает список единиц.</summary>
    [Fact]
    public async Task GetUnitsAsync_Returns_List()
    {
        const string json = "[{\"id\":1,\"name\":\"шт\"}]";
        var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var client = new LookupsClient(http);

        var result = await client.GetUnitsAsync();

        result.Should().HaveCount(1);
        result.First().Name.Should().Be("шт");
        handler.LastRequest!.RequestUri!.ToString().Should().Be("http://localhost/api/Lookups/units");
    }

    /// <summary>CreateUnitAsync отправляет POST и возвращает созданный объект.</summary>
    [Fact]
    public async Task CreateUnitAsync_Returns_Created()
    {
        const string json = "{\"id\":3,\"name\":\"кг\"}";
        var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var client = new LookupsClient(http);

        var result = await client.CreateUnitAsync(new UnitOfMeasureCreateDto { Name = "кг" });

        result.Id.Should().Be(3);
        result.Name.Should().Be("кг");
    }
}
