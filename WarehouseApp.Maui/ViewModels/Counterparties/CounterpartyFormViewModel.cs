using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Application.Abstractions;
using WarehouseApp.Maui.Services;

namespace WarehouseApp.Maui.ViewModels.Counterparties
{
    public partial class CounterpartyFormViewModel : ObservableObject
    {
        private readonly IPopupService _popupService;
        private readonly ICounterpartyApiService _apiService;
        private Counterparty? _entityToUpdate;

        [ObservableProperty] private string title = string.Empty;
        [ObservableProperty] private string buttonText = string.Empty;
        [ObservableProperty] private Color buttonColor = Colors.Transparent;
        [ObservableProperty] private string name = string.Empty;
        [ObservableProperty] private string phone = string.Empty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string errorMessage = string.Empty;

        public bool IsEditMode { get; }

        // Конструктор для СОЗДАНИЯ
        public CounterpartyFormViewModel(IPopupService popupService, ICounterpartyApiService apiService)
        {
            _popupService = popupService;
            _apiService = apiService;

            IsEditMode = false;
            Title = "Создать контрагента";
            ButtonText = "Добавить контрагента";
            ButtonColor = Color.FromArgb("#4CAF50");
        }

        // Конструктор для РЕДАКТИРОВАНИЯ
        public CounterpartyFormViewModel(IPopupService popupService, ICounterpartyApiService apiService, Counterparty existingCounterparty)
        {
            _popupService = popupService;
            _apiService = apiService;
            _entityToUpdate = existingCounterparty;

            IsEditMode = true;
            Title = "Контрагент";
            ButtonText = "Изменить контрагента";
            ButtonColor = Color.FromArgb("#1976D2");

            Name = existingCounterparty.Name;
            Phone = existingCounterparty.Phone ?? string.Empty;
            Email = existingCounterparty.Email ?? string.Empty;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ErrorMessage = "Поле «Наименование» обязательно для заполнения.";
                return;
            }

            ErrorMessage = string.Empty;

            try
            {
                string? phoneToSave = string.IsNullOrWhiteSpace(Phone) ? null : Phone;
                string? emailToSave = string.IsNullOrWhiteSpace(Email) ? null : Email;

                if (IsEditMode && _entityToUpdate != null)
                {
                    _entityToUpdate.Name = Name.Trim();
                    _entityToUpdate.Phone = phoneToSave;
                    _entityToUpdate.Email = emailToSave;
                    await _apiService.UpdateAsync(_entityToUpdate);
                }
                else
                {
                    var newCounterparty = new Counterparty
                    {
                        Name = Name.Trim(),
                        Phone = phoneToSave,
                        Email = emailToSave
                    };
                    await _apiService.AddAsync(newCounterparty);
                }

                if (Shell.Current != null)
                {
                    await _popupService.ClosePopupAsync(Shell.Current.Navigation);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка сохранения: {ex.Message}";
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        partial void OnErrorMessageChanged(string value)
        {
            OnPropertyChanged(nameof(HasError));
        }
    }
}
