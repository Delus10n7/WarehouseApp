using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WarehouseApp.Application.Dtos.CounterpartyDtos;
using WarehouseApp.Application.Services;

namespace WarehouseApp.Maui.ViewModels.Counterparties;

/// <summary>
/// ViewModel страницы списка контрагентов. Загрузка, поиск, событие редактирования.
/// </summary>
public partial class CounterpartiesViewModel : ObservableObject
{
    private readonly ICounterpartyApiService _apiService;
    private readonly ILogger<CounterpartiesViewModel> _logger;

    private List<CounterpartyDto> _allCounterparties = new();

    /// <summary>Флаг «идёт обновление» (для RefreshView).</summary>
    [ObservableProperty] private bool isRefreshing;

    /// <summary>Текст поиска по имени.</summary>
    [ObservableProperty] private string searchText = string.Empty;

    /// <summary>Отфильтрованный список контрагентов, привязанный к UI.</summary>
    public ObservableCollection<CounterpartyDto> Counterparties { get; } = new();

    /// <summary>Событие: пользователь нажал на карточку — нужно открыть форму редактирования.</summary>
    public event Func<CounterpartyDto, Task>? EditRequested;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="CounterpartiesViewModel"/>.
    /// </summary>
    public CounterpartiesViewModel(
        ICounterpartyApiService apiService,
        ILogger<CounterpartiesViewModel> logger)
    {
        _apiService = apiService;
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
            _allCounterparties = await _apiService.GetAllAsync();
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
