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

namespace WarehouseApp.Maui.ViewModels.Counterparties;

/// <summary>
/// ViewModel для формы создания и редактирования контрагента.
/// Содержит логику валидации и сохранения данных.
/// </summary>
public partial class CounterpartyFormViewModel : ObservableObject
{
    private readonly IPopupService _popupService;
    private readonly ICounterpartyApiService _apiService;
    private Counterparty? _entityToUpdate;

    /// <summary>Заголовок окна (меняется в зависимости от режима).</summary>
    [ObservableProperty] private string title = string.Empty;

    /// <summary>Текст на кнопке сохранения.</summary>
    [ObservableProperty] private string buttonText = string.Empty;

    /// <summary>Цвет кнопки сохранения.</summary>
    [ObservableProperty] private Color buttonColor = Colors.Transparent;

    /// <summary>Имя контрагента.</summary>
    [ObservableProperty] private string name = string.Empty;

    /// <summary>Телефон контрагента.</summary>
    [ObservableProperty] private string phone = string.Empty;

    /// <summary>Email контрагента.</summary>
    [ObservableProperty] private string email = string.Empty;

    /// <summary>Текст сообщения об ошибке валидации или сохранения.</summary>
    [ObservableProperty] private string errorMessage = string.Empty;

    /// <summary>Флаг, указывающий, находится ли форма в режиме редактирования.</summary>
    public bool IsEditMode { get; }

    /// <summary>
    /// Конструктор для создания нового контрагента.
    /// </summary>
    public CounterpartyFormViewModel(IPopupService popupService, ICounterpartyApiService apiService)
    {
        _popupService = popupService;
        _apiService = apiService;

        IsEditMode = false;
        Title = "Создать контрагента";
        ButtonText = "Добавить контрагента";
        ButtonColor = Color.FromArgb("#4CAF50");
    }

    /// <summary>
    /// Конструктор для редактирования существующего контрагента.
    /// </summary>
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

    /// <summary>
    /// Команда сохранения данных. Выполняет валидацию и отправляет запрос на сервер.
    /// </summary>
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

    /// <summary>Флаг наличия ошибки для отображения в UI.</summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Вызывается при изменении текста ошибки. Уведомляет UI об изменении свойства HasError.
    /// </summary>
    partial void OnErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasError));
    }
}
