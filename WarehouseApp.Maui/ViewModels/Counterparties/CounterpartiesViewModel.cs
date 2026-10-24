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

public partial class CounterpartiesViewModel : ObservableObject
{
    private readonly ICounterpartyApiService _apiService;
    private List<Counterparty> _allCounterparties = new();

    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private string searchText = string.Empty;
    public ObservableCollection<Counterparty> Counterparties { get; } = new();

    public event Func<Counterparty, Task>? EditRequested;

    public CounterpartiesViewModel(ICounterpartyApiService apiService)
    {
        _apiService = apiService;
    }

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

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

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

    [RelayCommand]
    private async Task Edit(Counterparty counterparty)
    {
        if (EditRequested != null)
            await EditRequested.Invoke(counterparty);
    }
}
