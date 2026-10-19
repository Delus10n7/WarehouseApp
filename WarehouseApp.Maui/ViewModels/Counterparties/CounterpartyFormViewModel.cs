using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using WarehouseApp.Domain.Entities;

namespace WarehouseApp.Maui.ViewModels.Counterparties
{
    public partial class CounterpartyFormViewModel : ObservableObject
    {
        private readonly IPopupService _popupService;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string buttonText;

        [ObservableProperty]
        private Color buttonColor;

        // Поля формы
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string phone;

        [ObservableProperty]
        private string email;

        public bool IsEditMode { get; }
        public Func<Task>? ClosePopupAction { get; set; }

        // Конструктор для режима "Создание"
        public CounterpartyFormViewModel(IPopupService popupService)
        {
            _popupService = popupService;

            IsEditMode = false;
            Title = "Создать контрагента";
            ButtonText = "Добавить контрагента";
            ButtonColor = Color.FromArgb("#4CAF50"); // Success из вашей палитры
            Name = string.Empty;
            Phone = string.Empty;
            Email = string.Empty;
        }

        // Конструктор для режима "Редактирование"
        public CounterpartyFormViewModel(IPopupService popupService, Counterparty existingCounterparty)
        {
            _popupService = popupService;

            IsEditMode = true;
            Title = "Контрагент";
            ButtonText = "Изменить контрагента";
            ButtonColor = Color.FromArgb("#1976D2"); // Primary из вашей палитры

            // Предзаполняем данные
            Name = existingCounterparty.Name;
            Phone = existingCounterparty.Phone ?? string.Empty;
            Email = existingCounterparty.Email ?? string.Empty;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                // Пока просто выходим, если поле пустое.
                // Позже здесь будет валидация.
                return;
            }

            if (IsEditMode)
            {
                // TODO: Логика обновления в БД
            }
            else
            {
                // TODO: Логика добавления в БД
            }

            // Закрываем попап после сохранения
            await _popupService.ClosePopupAsync(Application.Current!.Windows[0].Page!.Navigation);
        }
    }
}
