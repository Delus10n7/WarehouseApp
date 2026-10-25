using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Maui.Services;
using Xunit;

namespace WarehouseApp.Tests.Services;

/// <summary>
/// Тесты для <see cref="CounterpartyApiService"/>. 
/// Проверяют формирование URL, HTTP-методы и обработку ошибок.
/// </summary>
public class CounterpartyApiServiceTests
{
    [Fact]
    public async Task GetAllAsync_Should_Return_List_When_Response_Is_Success()
    {
        // Arrange
        var fakeJson = "[{\"id\":1,\"name\":\"Тест\"}]";
        var mockHandler = new MockHttpMessageHandler(fakeJson, HttpStatusCode.OK);

        var httpClient = new HttpClient(mockHandler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var service = new CounterpartyApiService(httpClient);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Тест");

        // Проверяем, что запрос ушел на правильный URL
        mockHandler.LastRequestUri.Should().Be("http://localhost/api/counterparties");
    }

    [Fact]
    public async Task AddAsync_Should_Throw_Exception_When_Server_Returns_Error()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler("", HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("http://localhost/") };
        var service = new CounterpartyApiService(httpClient);

        var newCounterparty = new Counterparty { Name = "Тест" };

        // Act
        Func<Task> act = async () => await service.AddAsync(newCounterparty);

        // Assert
        // Метод EnsureSuccessStatusCode должен выбросить HttpRequestException
        await act.Should().ThrowAsync<HttpRequestException>();
    }
}

/// <summary>
/// Вспомогательный класс для перехвата HTTP-запросов в тестах.
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly string _responseContent;
    private readonly HttpStatusCode _statusCode;

    public Uri? LastRequestUri { get; private set; }

    public MockHttpMessageHandler(string responseContent, HttpStatusCode statusCode)
    {
        _responseContent = responseContent;
        _statusCode = statusCode;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Запоминаем URL, по которому был отправлен запрос
        LastRequestUri = request.RequestUri;

        // Возвращаем подготовленный ответ
        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseContent, System.Text.Encoding.UTF8, "application/json")
        };

        return Task.FromResult(response);
    }
}
