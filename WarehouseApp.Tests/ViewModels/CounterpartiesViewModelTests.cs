using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WarehouseApp.Application.Dtos.CounterpartyDtos;
using WarehouseApp.Infrastructure.Client.Generated;
using WarehouseApp.Maui.ViewModels.Counterparties;

namespace WarehouseApp.Tests.ViewModels;

/// <summary>
/// Unit-тесты <see cref="CounterpartiesViewModel"/>.
/// </summary>
public class CounterpartiesViewModelTests
{
    private readonly ICounterpartiesClient _clientMock;
    private readonly ILogger<CounterpartiesViewModel> _loggerMock;

    /// <summary>Инициализирует моки.</summary>
    public CounterpartiesViewModelTests()
    {
        _clientMock = Substitute.For<ICounterpartiesClient>();
        _loggerMock = Substitute.For<ILogger<CounterpartiesViewModel>>();
    }

    /// <summary>LoadDataAsync заполняет Counterparties и снимает IsRefreshing.</summary>
    [Fact]
    public async Task LoadDataAsync_Populates_Collection_On_Success()
    {
        // Arrange — мокаем БЕСПАРАМЕТРИЧЕСКУЮ перегрузку (её вызывает VM)
        var fake = new List<CounterpartyDto>
        {
            new() { Id = 1, Name = "Ромашка" },
            new() { Id = 2, Name = "Иванов" }
        };
        _clientMock.GetAllAsync()
            .Returns(Task.FromResult<ICollection<CounterpartyDto>>(fake));

        var vm = new CounterpartiesViewModel(_clientMock, _loggerMock);

        // Act
        await vm.LoadDataAsync();

        // Assert
        vm.Counterparties.Should().HaveCount(2);
        vm.IsRefreshing.Should().BeFalse();
    }

    /// <summary>LoadDataAsync при исключении очищает коллекцию и снимает IsRefreshing.</summary>
    [Fact]
    public async Task LoadDataAsync_Clears_Collection_On_Error()
    {
        // Arrange
        _clientMock.GetAllAsync()
            .Returns(Task.FromException<ICollection<CounterpartyDto>>(
                new HttpRequestException("network error")));

        var vm = new CounterpartiesViewModel(_clientMock, _loggerMock);

        // Act
        await vm.LoadDataAsync();

        // Assert
        vm.Counterparties.Should().BeEmpty();
        vm.IsRefreshing.Should().BeFalse();
    }

    /// <summary>Изменение SearchText фильтрует коллекцию по имени.</summary>
    [Fact]
    public async Task OnSearchTextChanged_Filters_Collection()
    {
        // Arrange
        _clientMock.GetAllAsync()
            .Returns(Task.FromResult<ICollection<CounterpartyDto>>(new List<CounterpartyDto>
            {
                new() { Name = "ООО Ромашка" },
                new() { Name = "ИП Иванов" }
            }));

        var vm = new CounterpartiesViewModel(_clientMock, _loggerMock);
        await vm.LoadDataAsync();

        // Act
        vm.SearchText = "Ромаш";

        // Assert
        vm.Counterparties.Should().HaveCount(1);
        vm.Counterparties[0].Name.Should().Be("ООО Ромашка");
    }

    /// <summary>Edit поднимает событие EditRequested с переданным DTO.</summary>
    [Fact]
    public async Task Edit_Raises_EditRequested_With_Dto()
    {
        var vm = new CounterpartiesViewModel(_clientMock, _loggerMock);
        var captured = new List<CounterpartyDto>();
        vm.EditRequested += dto => { captured.Add(dto); return Task.CompletedTask; };

        var counterparty = new CounterpartyDto { Id = 42, Name = "Тест" };
        await vm.EditCommand.ExecuteAsync(counterparty);

        captured.Should().ContainSingle().Which.Should().BeSameAs(counterparty);
    }
}
