using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WarehouseApp.Application.Dtos.CounterpartyDtos;
using WarehouseApp.Application.Dtos.WareDocumentDtos;
using WarehouseApp.Domain.Entities.Enums;
using WarehouseApp.Infrastructure.Client.Generated;

namespace WarehouseApp.Maui.ViewModels.Documents;

/// <summary>
/// ViewModel страницы списка документов.
/// </summary>
public partial class DocumentsViewModel : ObservableObject
{
    private readonly IDocumentsClient _documentsClient;
    private readonly ICounterpartiesClient _counterpartiesClient;
    private readonly ILogger<DocumentsViewModel> _logger;

    private List<WareDocumentDto> _allDocuments = new();
    private bool _isLoading;

    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private bool isFilterVisible;

    // Фильтры
    [ObservableProperty] private string? selectedFilterType = "Все";
    [ObservableProperty] private CounterpartyDto? selectedFilterCounterparty;
    [ObservableProperty] private string? selectedSort = "По умолчанию";

    public ObservableCollection<WareDocumentDto> Documents { get; } = new();
    public ObservableCollection<CounterpartyDto> AvailableCounterparties { get; } = new();

    public List<string> TypeOptions { get; } = new() { "Все", "Поступление", "Продажа" };
    public List<string> SortOptions { get; } = new() { "По умолчанию", "По убыванию", "По возрастанию" };

    public event Func<WareDocumentDto?, Task>? OpenDocumentFormRequested;

    public DocumentsViewModel(
        IDocumentsClient documentsClient,
        ICounterpartiesClient counterpartiesClient,
        ILogger<DocumentsViewModel> logger)
    {
        _documentsClient = documentsClient;
        _counterpartiesClient = counterpartiesClient;
        _logger = logger;
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        if (_isLoading) return;
        _isLoading = true;
        IsRefreshing = true;

        try
        {
            var docsTask = _documentsClient.GetAllAsync();
            var cpTask = _counterpartiesClient.GetAllAsync();
            await Task.WhenAll(docsTask, cpTask);

            _allDocuments = (await docsTask).ToList();

            AvailableCounterparties.Clear();
            foreach (var cp in await cpTask) AvailableCounterparties.Add(cp);

            ApplyFilters();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки документов");
            _allDocuments = new();
            ApplyFilters();
        }
        finally
        {
            IsRefreshing = false;
            _isLoading = false;
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnSelectedFilterTypeChanged(string? value) => ApplyFilters();
    partial void OnSelectedFilterCounterpartyChanged(CounterpartyDto? value) => ApplyFilters();
    partial void OnSelectedSortChanged(string? value) => ApplyFilters();

    [RelayCommand]
    private void ToggleFilter() => IsFilterVisible = !IsFilterVisible;

    [RelayCommand]
    public void ApplyFilters()
    {
        var filtered = _allDocuments.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
            filtered = filtered.Where(d =>
                d.Number.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (d.CounterpartyName != null && d.CounterpartyName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));

        if (SelectedFilterType == "Поступление")
            filtered = filtered.Where(d => d.Type == WareDocumentType.Receipt);
        else if (SelectedFilterType == "Продажа")
            filtered = filtered.Where(d => d.Type == WareDocumentType.Sale);

        if (SelectedFilterCounterparty != null)
            filtered = filtered.Where(d => d.CounterpartyId == SelectedFilterCounterparty.Id);

        if (SelectedSort == "По убыванию")
            filtered = filtered.OrderByDescending(d => d.TotalAmount);
        else if (SelectedSort == "По возрастанию")
            filtered = filtered.OrderBy(d => d.TotalAmount);

        Documents.Clear();
        foreach (var item in filtered) Documents.Add(item);
    }

    [RelayCommand]
    public void ResetFilters()
    {
        SearchText = string.Empty;
        SelectedFilterType = "Все";
        SelectedFilterCounterparty = null;
        SelectedSort = "По умолчанию";
        ApplyFilters();
    }

    [RelayCommand]
    private async Task Edit(WareDocumentDto? document)
    {
        if (OpenDocumentFormRequested != null)
            await OpenDocumentFormRequested.Invoke(document);
    }
}
