using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WarehouseApp.Application.Dtos.CounterpartyDtos;
using WarehouseApp.Infrastructure.Client.Generated;

namespace WarehouseApp.Maui.ViewModels.Counterparties;

/// <summary>
/// ViewModel формы создания/редактирования контрагента с валидацией и сохранением.
/// </summary>
public partial class CounterpartyFormViewModel : ObservableObject
{
    private readonly IPopupService _popupService;
    private readonly ICounterpartiesClient _counterpartiesClient;
    private readonly CounterpartyDto? _entityToUpdate;

    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string buttonText = string.Empty;
    [ObservableProperty] private Color buttonColor = Colors.Transparent;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string phone = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string errorMessage = string.Empty;

    public bool IsEditMode => _entityToUpdate != null;
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>Конструктор для создания нового контрагента.</summary>
    public CounterpartyFormViewModel(
        IPopupService popupService,
        ICounterpartiesClient counterpartiesClient)
    {
        _popupService = popupService;
        _counterpartiesClient = counterpartiesClient;

        Title = "Создать контрагента";
        ButtonText = "Добавить контрагента";
        ButtonColor = Color.FromArgb("#4CAF50");
    }

    /// <summary>Конструктор для редактирования существующего контрагента.</summary>
    public CounterpartyFormViewModel(
        IPopupService popupService,
        ICounterpartiesClient counterpartiesClient,
        CounterpartyDto existing)
    {
        _popupService = popupService;
        _counterpartiesClient = counterpartiesClient;
        _entityToUpdate = existing;

        Title = "Контрагент";
        ButtonText = "Изменить контрагента";
        ButtonColor = Color.FromArgb("#1976D2");

        Name = existing.Name;
        Phone = existing.Phone ?? string.Empty;
        Email = existing.Email ?? string.Empty;
    }

    /// <summary>
    /// Валидирует форму и сохраняет контрагента (создание или обновление).
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
            var dto = new CounterpartyRequestDto
            {
                Name = Name.Trim(),
                Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim()
            };

            if (IsEditMode && _entityToUpdate != null)
                await _counterpartiesClient.UpdateAsync(_entityToUpdate.Id, dto);
            else
                await _counterpartiesClient.CreateAsync(dto);

            if (Shell.Current != null)
                await _popupService.ClosePopupAsync(Shell.Current.Navigation);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ошибка сохранения: {ex.Message}";
        }
    }

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));
}
