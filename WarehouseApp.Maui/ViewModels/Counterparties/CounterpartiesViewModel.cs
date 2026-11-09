using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WarehouseApp.Application.Dtos.CounterpartyDtos;
using WarehouseApp.Infrastructure.Client.Generated;

namespace WarehouseApp.Maui.ViewModels.Counterparties;

/// <summary>
/// ViewModel страницы списка контрагентов. Загрузка, поиск, событие редактирования.
/// </summary>
public partial class CounterpartiesViewModel : ObservableObject
{
    private readonly ICounterpartiesClient _counterpartiesClient;
    private readonly ILogger<CounterpartiesViewModel> _logger;

    private List<CounterpartyDto> _allCounterparties = new();

    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private string searchText = string.Empty;

    /// <summary>Отфильтрованный список контрагентов, привязанный к UI.</summary>
    public ObservableCollection<CounterpartyDto> Counterparties { get; } = new();

    /// <summary>Событие: пользователь нажал на карточку — нужно открыть форму редактирования.</summary>
    public event Func<CounterpartyDto, Task>? EditRequested;

    public CounterpartiesViewModel(
        ICounterpartiesClient counterpartiesClient,
        ILogger<CounterpartiesViewModel> logger)
    {
        _counterpartiesClient = counterpartiesClient;
        _logger = logger;
    }

    /// <summary>
    /// Асинхронно загружает список контрагентов с сервера.
    /// </summary>
    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsRefreshing = true;
        try
        {
            // GetAllAsync возвращает ICollection<T>, приводим к List
            _allCounterparties = (await _counterpartiesClient.GetAllAsync()).ToList();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки контрагентов");
            _allCounterparties = new();
            ApplyFilter();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    /// <summary>
    /// Фильтрует исходный список по введённому тексту.
    /// </summary>
    private void ApplyFilter()
    {
        Counterparties.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allCounterparties
            : _allCounterparties.Where(c =>
                c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var item in filtered) Counterparties.Add(item);
    }

    /// <summary>
    /// Команда редактирования: поднимает событие <see cref="EditRequested"/>.
    /// </summary>
    [RelayCommand]
    private async Task Edit(CounterpartyDto counterparty)
    {
        if (EditRequested != null)
            await EditRequested.Invoke(counterparty);
    }
}
