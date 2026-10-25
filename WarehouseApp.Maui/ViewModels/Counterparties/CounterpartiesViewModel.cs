using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Maui.Services;

namespace WarehouseApp.Maui.ViewModels.Counterparties;

/// <summary>
/// ViewModel для страницы списка контрагентов. 
/// Управляет загрузкой данных, фильтрацией и обработкой событий.
/// </summary>
public partial class CounterpartiesViewModel : ObservableObject
{
    private readonly ICounterpartyApiService _apiService;
    private List<Counterparty> _allCounterparties = new();

    /// <summary>Индикатор того, что в данный момент выполняется загрузка данных.</summary>
    [ObservableProperty] private bool isRefreshing;

    /// <summary>Текст для поиска и фильтрации контрагентов по имени.</summary>
    [ObservableProperty] private string searchText = string.Empty;

    /// <summary>Коллекция отфильтрованных контрагентов, привязанная к UI.</summary>
    public ObservableCollection<Counterparty> Counterparties { get; } = new();

    /// <summary>
    /// Событие, запрашивающее открытие формы редактирования для выбранного контрагента.
    /// </summary>
    public event Func<Counterparty, Task>? EditRequested;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CounterpartiesViewModel"/>.
    /// </summary>
    /// <param name="apiService">Сервис для работы с API.</param>
    public CounterpartiesViewModel(ICounterpartyApiService apiService)
    {
        _apiService = apiService;
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
            System.Diagnostics.Debug.WriteLine($"Ошибка API: {ex.Message}");
            _allCounterparties = new List<Counterparty>();
            ApplyFilter();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Вызывается при изменении текста в поле поиска. Применяет фильтрацию.
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    /// <summary>
    /// Фильтрует исходный список контрагентов на основе введенного текста поиска.
    /// </summary>
    private void ApplyFilter()
    {
        Counterparties.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allCounterparties
            : _allCounterparties.Where(c => c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var item in filtered)
        {
            Counterparties.Add(item);
        }
    }

    /// <summary>
    /// Команда для вызова события редактирования контрагента.
    /// </summary>
    /// <param name="counterparty">Контрагент для редактирования.</param>
    [RelayCommand]
    private async Task Edit(Counterparty counterparty)
    {
        if (EditRequested != null)
            await EditRequested.Invoke(counterparty);
    }
}
