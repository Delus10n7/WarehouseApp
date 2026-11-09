using System.Net;
using FluentAssertions;
using WarehouseApp.Application.Dtos.ProductDtos;
using WarehouseApp.Infrastructure.Client.Generated;
using WarehouseApp.Tests.TestHelpers;

namespace WarehouseApp.Tests.Infrastructure.Client;

/// <summary>
/// Unit-тесты NSwag-сгенерированного <see cref="ProductsClient"/>.
/// </summary>
public class ProductsClientTests
{
    /// <summary>GetAllAsync парсит массив товаров с плоскими CategoryName / UnitOfMeasureName.</summary>
    [Fact]
    public async Task GetAllAsync_Parses_Products_With_Flat_Names()
    {
        const string json = """
        [
            {
                "id": 1,
                "name": "Телефон",
                "article": "TEL-1",
                "categoryId": 3,
                "categoryName": "Электроника",
                "unitOfMeasureId": 1,
                "unitOfMeasureName": "шт",
                "quantity": 5.5,
                "purchasePrice": 1000,
                "salePrice": 1500
            }
        ]
        """;
        var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var client = new ProductsClient(http);

        var result = await client.GetAllAsync();

        result.Should().HaveCount(1);
        var p = result.First();
        p.Name.Should().Be("Телефон");
        p.CategoryName.Should().Be("Электроника");
        p.UnitOfMeasureName.Should().Be("шт");
        p.Quantity.Should().Be(5.5m);
        p.PurchasePrice.Should().Be(1000m);
        p.SalePrice.Should().Be(1500m);

        handler.LastRequest!.RequestUri!.ToString().Should().Be("http://localhost/api/Products");
    }

    /// <summary>CreateAsync отправляет POST и обрабатывает 201.</summary>
    [Fact]
    public async Task CreateAsync_Handles_201_Created()
    {
        const string json = "{\"id\":42,\"name\":\"Новый\",\"article\":\"N-1\"}";
        var handler = new MockHttpMessageHandler(json, HttpStatusCode.Created);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var client = new ProductsClient(http);

        var result = await client.CreateAsync(new ProductRequestDto
        {
            Name = "Новый",
            Article = "N-1",
            CategoryId = 1,
            UnitOfMeasureId = 1
        });

        result.Id.Should().Be(42);
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);
        handler.LastRequestBody.Should().Contain("\"article\":\"N-1\"");
    }

    /// <summary>UpdateAsync отправляет PUT и обрабатывает 204.</summary>
    [Fact]
    public async Task UpdateAsync_Handles_204()
    {
        var handler = new MockHttpMessageHandler("", HttpStatusCode.NoContent);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var client = new ProductsClient(http);

        await client.UpdateAsync(5, new ProductRequestDto { Name = "X", Article = "X" });

        handler.LastRequest!.Method.Should().Be(HttpMethod.Put);
        handler.LastRequest!.RequestUri!.ToString().Should().Be("http://localhost/api/Products/5");
    }

    /// <summary>DeleteAsync отправляет DELETE и обрабатывает 204.</summary>
    [Fact]
    public async Task DeleteAsync_Handles_204()
    {
        var handler = new MockHttpMessageHandler("", HttpStatusCode.NoContent);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var client = new ProductsClient(http);

        await client.DeleteAsync(3);

        handler.LastRequest!.Method.Should().Be(HttpMethod.Delete);
    }

    /// <summary>CreateAsync бросает ApiException на 400.</summary>
    [Fact]
    public async Task CreateAsync_Throws_On_400()
    {
        var handler = new MockHttpMessageHandler("Bad request", HttpStatusCode.BadRequest);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var client = new ProductsClient(http);

        var act = async () => await client.CreateAsync(new ProductRequestDto());

        await act.Should().ThrowAsync<ApiException>()
            .Where(ex => ex.StatusCode == 400);
    }
}
