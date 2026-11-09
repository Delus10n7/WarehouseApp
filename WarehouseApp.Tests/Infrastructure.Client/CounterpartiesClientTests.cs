using System.Net;
using System.Text.Json;
using FluentAssertions;
using WarehouseApp.Application.Dtos.CounterpartyDtos;
using WarehouseApp.Infrastructure.Client.Generated;
using WarehouseApp.Tests.TestHelpers;

namespace WarehouseApp.Tests.Infrastructure.Client;

/// <summary>
/// Unit-тесты NSwag-сгенерированного <see cref="CounterpartiesClient"/>.
/// </summary>
/// <remarks>
/// Используем <see cref="MockHttpMessageHandler"/>, чтобы перехватить HTTP-запрос
/// и вернуть подготовленный ответ. Так проверяется:
/// <list type="bullet">
///   <item><description>формирование URL и метода HTTP;</description></item>
///   <item><description>десериализация JSON (в том числе camelCase → PascalCase);</description></item>
///   <item><description>обработка ошибок (4xx/5xx → ApiException).</description></item>
/// </list>
/// </remarks>
public class CounterpartiesClientTests
{
    /// <summary>
    /// <c>GetAllAsync</c> возвращает данные и корректно десериализует camelCase.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_Parses_Response_And_Uses_CamelCase_Insensitive()
    {
        // Сервер отдаёт camelCase — клиент должен корректно заполнить PascalCase-свойства
        const string json = "[{\"id\":1,\"name\":\"Тест\",\"phone\":\"+7\",\"email\":\"a@b.c\"}]";
        var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var client = new CounterpartiesClient(http);

        var result = await client.GetAllAsync();

        result.Should().HaveCount(1);
        result.First().Id.Should().Be(1);
        result.First().Name.Should().Be("Тест");        // camelCase → PascalCase
        result.First().Phone.Should().Be("+7");
        result.First().Email.Should().Be("a@b.c");

        // Проверяем корректный URL
        handler.LastRequest!.RequestUri!.ToString().Should().Be("http://localhost/api/Counterparties");
        handler.LastRequest!.Method.Should().Be(HttpMethod.Get);
    }

    /// <summary>
    /// <c>CreateAsync</c> отправляет POST и возвращает созданный объект (201).
    /// </summary>
    [Fact]
    public async Task CreateAsync_Sends_Post_And_Returns_Created()
    {
        const string json = "{\"id\":42,\"name\":\"Новый\"}";
        var handler = new MockHttpMessageHandler(json, HttpStatusCode.Created);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var client = new CounterpartiesClient(http);

        var dto = new CounterpartyRequestDto { Name = "Новый" };
        var result = await client.CreateAsync(dto);

        result.Id.Should().Be(42);
        result.Name.Should().Be("Новый");
        handler.LastRequest!.Method.Should().Be(HttpMethod.Post);

        // Проверяем тело запроса через десериализацию (обход escape-последовательностей)
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        doc.RootElement.GetProperty("name").GetString().Should().Be("Новый");
    }

    /// <summary>
    /// <c>GetByIdAsync</c> возвращает объект.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_Returns_Object()
    {
        const string json = "{\"id\":7,\"name\":\"Найдено\"}";
        var handler = new MockHttpMessageHandler(json, HttpStatusCode.OK);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var client = new CounterpartiesClient(http);

        var result = await client.GetByIdAsync(7);

        result.Id.Should().Be(7);
        result.Name.Should().Be("Найдено");
        handler.LastRequest!.RequestUri!.ToString().Should().Be("http://localhost/api/Counterparties/7");
    }

    /// <summary>
    /// <c>UpdateAsync</c> отправляет PUT и не падает при 204.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_Sends_Put_And_Handles_NoContent()
    {
        var handler = new MockHttpMessageHandler("", HttpStatusCode.NoContent);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var client = new CounterpartiesClient(http);

        var dto = new CounterpartyRequestDto { Name = "X" };
        await client.UpdateAsync(5, dto);

        handler.LastRequest!.Method.Should().Be(HttpMethod.Put);
        handler.LastRequest!.RequestUri!.ToString().Should().Be("http://localhost/api/Counterparties/5");
    }

    /// <summary>
    /// <c>DeleteAsync</c> отправляет DELETE и не падает при 204.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_Sends_Delete_And_Handles_NoContent()
    {
        var handler = new MockHttpMessageHandler("", HttpStatusCode.NoContent);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var client = new CounterpartiesClient(http);

        await client.DeleteAsync(3);

        handler.LastRequest!.Method.Should().Be(HttpMethod.Delete);
        handler.LastRequest!.RequestUri!.ToString().Should().Be("http://localhost/api/Counterparties/3");
    }

    /// <summary>
    /// <c>CreateAsync</c> бросает <see cref="ApiException"/> при 500.
    /// </summary>
    [Fact]
    public async Task CreateAsync_Throws_ApiException_On_Server_Error()
    {
        var handler = new MockHttpMessageHandler("Server error", HttpStatusCode.InternalServerError);
        var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var client = new CounterpartiesClient(http);

        var act = async () => await client.CreateAsync(new CounterpartyRequestDto { Name = "X" });

        await act.Should().ThrowAsync<ApiException>()
            .Where(ex => ex.StatusCode == 500);
    }
}
