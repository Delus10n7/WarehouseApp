using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Interfaces.Abstractions;

namespace WarehouseApp.Maui.ViewModels.Counterparties
{
    public partial class CounterpartyFormViewModel : ObservableObject
    {
        private readonly IPopupService _popupService;
        private readonly ICounterpartyRepository _repository;
        private Counterparty? _entityToUpdate;

        [ObservableProperty] private string title = string.Empty;
        [ObservableProperty] private string buttonText = string.Empty;
        [ObservableProperty] private Color buttonColor = Colors.Transparent;
        [ObservableProperty] private string name = string.Empty;
        [ObservableProperty] private string phone = string.Empty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string errorMessage = string.Empty;

        public bool IsEditMode { get; }

        // Конструктор для режима "Создание"
        public CounterpartyFormViewModel(IPopupService popupService, ICounterpartyRepository repository)
        {
            _popupService = popupService;
            _repository = repository;

            IsEditMode = false;
            Title = "Создать контрагента";
            ButtonText = "Добавить контрагента";
            ButtonColor = Color.FromArgb("#4CAF50");
        }

        // Конструктор для режима "Редактирование"
        public CounterpartyFormViewModel(IPopupService popupService, ICounterpartyRepository repository, Counterparty existingCounterparty)
        {
            _popupService = popupService;
            _repository = repository;
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
                    await _repository.UpdateAsync(_entityToUpdate);
                }
                else
                {
                    var newCounterparty = new Counterparty
                    {
                        Name = Name.Trim(),
                        Phone = phoneToSave,
                        Email = emailToSave
                    };
                    await _repository.AddAsync(newCounterparty);
                }

                if (Shell.Current != null)
                {
                    await _popupService.ClosePopupAsync(Shell.Current.Navigation);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка сохранения: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        partial void OnErrorMessageChanged(string value)
        {
            OnPropertyChanged(nameof(HasError));
        }
    }
}
