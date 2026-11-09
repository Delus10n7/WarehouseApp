namespace WarehouseApp.Application.Dtos.CommonDtos;

/// <summary>
/// DTO ошибки в формате RFC 7807 (Problem Details for HTTP APIs).
/// Используется и сервером (через встроенный <c>Microsoft.AspNetCore.Mvc.ProblemDetails</c>),
/// и клиентом (для десериализации 4xx/5xx ответов).
/// </summary>
/// <remarks>
/// Название класса и набор свойств должны точно совпадать со схемой
/// <c>ProblemDetails</c> в <c>swagger.json</c>, иначе NSwag не найдёт тип
/// и сгенерирует ошибку компиляции (CS0246).
/// </remarks>
public class ProblemDetails
{
    /// <summary>URI, идентифицирующий тип проблемы.</summary>
    public string? Type { get; set; }

    /// <summary>Краткое человекочитаемое описание проблемы.</summary>
    public string? Title { get; set; }

    /// <summary>HTTP-статус код.</summary>
    public int? Status { get; set; }

    /// <summary>Подробное описание проблемы.</summary>
    public string? Detail { get; set; }

    /// <summary>URI, идентифицирующий конкретный экземпляр проблемы.</summary>
    public string? Instance { get; set; }
}
