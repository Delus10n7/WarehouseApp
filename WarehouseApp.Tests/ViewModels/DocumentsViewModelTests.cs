using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WarehouseApp.Application.Dtos.CounterpartyDtos;
using WarehouseApp.Application.Dtos.WareDocumentDtos;
using WarehouseApp.Domain.Entities.Enums;
using WarehouseApp.Infrastructure.Client.Generated;
using WarehouseApp.Maui.ViewModels.Documents;

namespace WarehouseApp.Tests.ViewModels;

/// <summary>
/// Unit-тесты <see cref="DocumentsViewModel"/>.
/// </summary>
public class DocumentsViewModelTests
{
    private readonly IDocumentsClient _documentsClient;
    private readonly ICounterpartiesClient _counterpartiesClient;
    private readonly ILogger<DocumentsViewModel> _logger;

    public DocumentsViewModelTests()
    {
        _documentsClient = Substitute.For<IDocumentsClient>();
        _counterpartiesClient = Substitute.For<ICounterpartiesClient>();
        _logger = Substitute.For<ILogger<DocumentsViewModel>>();
    }

    /// <summary>
    /// Вспомогательный метод для настройки успешной загрузки данных.
    /// </summary>
    private void Setup_Load_Success(List<WareDocumentDto>? docs = null, List<CounterpartyDto>? cps = null)
    {
        _documentsClient.GetAllAsync()
            .Returns(Task.FromResult<ICollection<WareDocumentDto>>(docs ?? new List<WareDocumentDto>()));
        _counterpartiesClient.GetAllAsync()
            .Returns(Task.FromResult<ICollection<CounterpartyDto>>(cps ?? new List<CounterpartyDto>()));
    }

    /// <summary>
    /// Проверяет, что LoadDataAsync заполняет коллекции и снимает индикатор загрузки.
    /// </summary>
    [Fact]
    public async Task LoadDataAsync_Populates_Collections()
    {
        Setup_Load_Success(
            new List<WareDocumentDto> { new() { Id = 1, Number = "DOC-1" } },
            new List<CounterpartyDto> { new() { Id = 1, Name = "CP-1" } });

        var vm = new DocumentsViewModel(_documentsClient, _counterpartiesClient, _logger);
        await vm.LoadDataAsync();

        vm.Documents.Should().HaveCount(1);
        vm.AvailableCounterparties.Should().HaveCount(1);
        vm.IsRefreshing.Should().BeFalse();
    }

    /// <summary>
    /// Проверяет, что фильтрация по типу документа оставляет только нужные записи.
    /// </summary>
    [Fact]
    public async Task ApplyFilters_Filters_By_Type()
    {
        Setup_Load_Success(new List<WareDocumentDto>
        {
            new() { Type = WareDocumentType.Receipt },
            new() { Type = WareDocumentType.Sale }
        });

        var vm = new DocumentsViewModel(_documentsClient, _counterpartiesClient, _logger);
        await vm.LoadDataAsync();

        vm.SelectedFilterType = "Продажа";

        vm.Documents.Should().HaveCount(1);
        vm.Documents[0].Type.Should().Be(WareDocumentType.Sale);
    }

    /// <summary>
    /// Проверяет, что фильтрация по контрагенту оставляет только его документы.
    /// </summary>
    [Fact]
    public async Task ApplyFilters_Filters_By_Counterparty()
    {
        Setup_Load_Success(
            new List<WareDocumentDto>
            {
                new() { CounterpartyId = 1, CounterpartyName = "A" },
                new() { CounterpartyId = 2, CounterpartyName = "B" }
            },
            new List<CounterpartyDto>
            {
                new() { Id = 1, Name = "A" },
                new() { Id = 2, Name = "B" }
            });

        var vm = new DocumentsViewModel(_documentsClient, _counterpartiesClient, _logger);
        await vm.LoadDataAsync();

        vm.SelectedFilterCounterparty = vm.AvailableCounterparties.First(c => c.Id == 2);

        vm.Documents.Should().HaveCount(1);
        vm.Documents[0].CounterpartyId.Should().Be(2);
    }

    /// <summary>
    /// Проверяет, что сортировка по сумме работает корректно.
    /// </summary>
    [Fact]
    public async Task ApplyFilters_Sorts_By_TotalAmount()
    {
        Setup_Load_Success(new List<WareDocumentDto>
        {
            new() { TotalAmount = 100 },
            new() { TotalAmount = 300 },
            new() { TotalAmount = 200 }
        });

        var vm = new DocumentsViewModel(_documentsClient, _counterpartiesClient, _logger);
        await vm.LoadDataAsync();

        vm.SelectedSort = "По убыванию";
        vm.Documents.Select(d => d.TotalAmount).Should().ContainInOrder(300m, 200m, 100m);

        vm.SelectedSort = "По возрастанию";
        vm.Documents.Select(d => d.TotalAmount).Should().ContainInOrder(100m, 200m, 300m);
    }

    /// <summary>
    /// Проверяет, что ResetFilters сбрасывает все фильтры и возвращает полный список.
    /// </summary>
    [Fact]
    public async Task ResetFilters_Clears_All_Filters()
    {
        Setup_Load_Success(new List<WareDocumentDto>
        {
            new() { Type = WareDocumentType.Receipt },
            new() { Type = WareDocumentType.Sale }
        });

        var vm = new DocumentsViewModel(_documentsClient, _counterpartiesClient, _logger);
        await vm.LoadDataAsync();

        vm.SelectedFilterType = "Продажа";
        vm.SearchText = "test";

        vm.ResetFilters();

        vm.SelectedFilterType.Should().Be("Все");
        vm.SearchText.Should().BeEmpty();
        vm.Documents.Should().HaveCount(2);
    }
}
