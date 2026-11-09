using System.Net;
using System.Text;

namespace WarehouseApp.Tests.TestHelpers;

/// <summary>
/// Подменный <see cref="HttpMessageHandler"/> для юнит-тестирования NSwag-клиентов.
/// Перехватывает HTTP-запросы и возвращает заранее подготовленный ответ.
/// </summary>
/// <remarks>
/// Позволяет:
/// <list type="bullet">
///   <item><description>проверить URL и метод запроса (<see cref="LastRequest"/>);</description></item>
///   <item><description>проверить тело POST/PUT (<see cref="LastRequestBody"/>);</description></item>
///   <item><description>вернуть произвольный статус и тело ответа без реальной сети.</description></item>
/// </list>
/// </remarks>
public class MockHttpMessageHandler : HttpMessageHandler
{
    /// <summary>
    /// Функция-ответчик, возвращающая <see cref="HttpResponseMessage"/> на запрос.
    /// </summary>
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

    /// <summary>
    /// Последний перехваченный HTTP-запрос (для проверки в тестах).
    /// </summary>
    public HttpRequestMessage? LastRequest { get; private set; }

    /// <summary>
    /// Тело последнего перехваченного запроса (только если Content не null).
    /// </summary>
    public string? LastRequestBody { get; private set; }

    /// <summary>
    /// Создаёт обработчик, всегда возвращающий фиксированный ответ.
    /// </summary>
    /// <param name="content">Тело ответа (ожидается JSON).</param>
    /// <param name="statusCode">HTTP-статус ответа.</param>
    public MockHttpMessageHandler(string content, HttpStatusCode statusCode)
        : this(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        })
    {
    }

    /// <summary>
    /// Создаёт обработчик с динамическим ответом.
    /// </summary>
    /// <param name="responder">Функция, вычисляющая ответ по запросу.</param>
    public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    /// <summary>
    /// Перехватывает HTTP-запрос, запоминает его и возвращает подготовленный ответ.
    /// </summary>
    /// <param name="request">HTTP-запрос от HttpClient.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Подготовленный <see cref="HttpResponseMessage"/>.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Сохраняем запрос для возможных проверок в тестах
        LastRequest = request;

        // Читаем тело, если оно есть (для проверки сериализации)
        if (request.Content != null)
            LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);

        // Возвращаем ответ через функцию-ответчик
        return _responder(request);
    }
}
