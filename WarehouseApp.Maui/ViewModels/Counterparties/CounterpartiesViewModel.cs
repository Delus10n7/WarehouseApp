using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Interfaces.Abstractions;

namespace WarehouseApp.Maui.ViewModels.Counterparties
{
    public partial class CounterpartiesViewModel : ObservableObject
    {
        private readonly ICounterpartyRepository _repository;
        private List<Counterparty> _allCounterparties = new();

        [ObservableProperty] private bool isRefreshing;
        [ObservableProperty] private string searchText = string.Empty;
        public ObservableCollection<Counterparty> Counterparties { get; } = new();

        public event Func<Counterparty, Task>? EditRequested;

        public CounterpartiesViewModel(ICounterpartyRepository repository)
        {
            _repository = repository;
        }

        [RelayCommand]
        public async Task LoadDataAsync()
        {
            IsRefreshing = true;
            try
            {
                _allCounterparties = await _repository.GetAllAsync();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки БД: {ex.Message}");

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
}
