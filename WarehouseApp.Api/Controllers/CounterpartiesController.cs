using Microsoft.AspNetCore.Mvc;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Application.Abstractions;

namespace WarehouseApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // URL /api/Counterparties
public class CounterpartiesController : ControllerBase
{
    private readonly ICounterpartyRepository _repository;

    public CounterpartiesController(ICounterpartyRepository repository)
    {
        _repository = repository;
    }

    // GET: api/counterparties
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Counterparty>>> GetAll()
    {
        var counterparties = await _repository.GetAllAsync();
        return Ok(counterparties);
    }

    // POST: api/counterparties
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] Counterparty counterparty)
    {
        await _repository.AddAsync(counterparty);
        // В реальном приложении лучше возвращать CreatedAtAction
        return Ok();
    }

    // PUT: api/counterparties/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Counterparty counterparty)
    {
        // Здесь должна быть проверка, что id совпадает
        await _repository.UpdateAsync(counterparty);
        return NoContent();
    }
}

