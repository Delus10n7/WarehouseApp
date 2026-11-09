using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WarehouseApp.Application.Dtos.CounterpartyDtos;
using WarehouseApp.Infrastructure.Client.Generated;

namespace WarehouseApp.Maui.ViewModels.Counterparties;

/// <summary>
/// ViewModel формы создания/редактирования контрагента.
/// </summary>
public partial class CounterpartyFormViewModel : ObservableObject
{
    /// <summary>Сервис управления попапами (закрытие окна после сохранения).</summary>
    private readonly IPopupService _popupService;

    /// <summary>Сгенерированный клиент API контрагентов.</summary>
    private readonly ICounterpartiesClient _counterpartiesClient;

    /// <summary>
    /// DTO редактируемого контрагента. <c>null</c> в режиме создания.
    /// </summary>
    private readonly CounterpartyDto? _entityToUpdate;

    /// <summary>Заголовок окна («Создать контрагента» / «Контрагент»).</summary>
    [ObservableProperty] private string title = string.Empty;

    /// <summary>Текст на кнопке сохранения.</summary>
    [ObservableProperty] private string buttonText = string.Empty;

    /// <summary>Цвет кнопки сохранения (зелёный для создания, синий для редактирования).</summary>
    [ObservableProperty] private Color buttonColor = Colors.Transparent;

    /// <summary>Имя контрагента — привязано к <c>Entry.Text</c>.</summary>
    [ObservableProperty] private string name = string.Empty;

    /// <summary>Телефон — привязано к <c>Entry.Text</c> (клавиатура Telephone).</summary>
    [ObservableProperty] private string phone = string.Empty;

    /// <summary>Email — привязано к <c>Entry.Text</c> (клавиатура Email).</summary>
    [ObservableProperty] private string email = string.Empty;

    /// <summary>Текст ошибки валидации или сохранения — привязан к <c>Label.Text</c>.</summary>
    [ObservableProperty] private string errorMessage = string.Empty;

    /// <summary>
    /// Признак режима редактирования (вычисляется из <see cref="_entityToUpdate"/>).
    /// </summary>
    public bool IsEditMode => _entityToUpdate != null;

    /// <summary>
    /// Признак наличия ошибки — используется для <c>Label.IsVisible</c>.
    /// </summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Конструктор для режима создания нового контрагента.
    /// </summary>
    /// <param name="popupService">Сервис управления попапами.</param>
    /// <param name="counterpartiesClient">Клиент API контрагентов.</param>
    public CounterpartyFormViewModel(
        IPopupService popupService,
        ICounterpartiesClient counterpartiesClient)
    {
        _popupService = popupService;
        _counterpartiesClient = counterpartiesClient;

        // Настройки под режим создания
        Title = "Создать контрагента";
        ButtonText = "Добавить контрагента";
        ButtonColor = Color.FromArgb("#4CAF50");    // зелёный — «создать»
    }

    /// <summary>
    /// Конструктор для режима редактирования существующего контрагента.
    /// </summary>
    /// <param name="popupService">Сервис управления попапами.</param>
    /// <param name="counterpartiesClient">Клиент API контрагентов.</param>
    /// <param name="existing">DTO существующего контрагента для редактирования.</param>
    public CounterpartyFormViewModel(
        IPopupService popupService,
        ICounterpartiesClient counterpartiesClient,
        CounterpartyDto existing)
    {
        _popupService = popupService;
        _counterpartiesClient = counterpartiesClient;
        _entityToUpdate = existing;

        // Настройки под режим редактирования
        Title = "Контрагент";
        ButtonText = "Изменить контрагента";
        ButtonColor = Color.FromArgb("#1976D2");    // синий — «изменить»

        // Заполняем поля текущими значениями (null → пустая строка, чтобы Entry не падал)
        Name = existing.Name;
        Phone = existing.Phone ?? string.Empty;
        Email = existing.Email ?? string.Empty;
    }

    /// <summary>
    /// Валидирует форму и сохраняет контрагента (создание или обновление).
    /// </summary>
    /// <returns>Задача, завершающаяся после сохранения (или после показа ошибки).</returns>
    [RelayCommand]
    private async Task SaveAsync()
    {
        // Валидация: имя обязательно
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Поле «Наименование» обязательно для заполнения.";
            return;                                 // выходим, не отправляя запрос
        }

        // Сбрасываем прошлую ошибку
        ErrorMessage = string.Empty;

        try
        {
            // Готовим DTO. Опциональные поля нормализуем: пустая строка → null,
            // чтобы в БД ушёл NULL, а не ""
            var dto = new CounterpartyRequestDto
            {
                Name = Name.Trim(),                                             // обрезаем пробелы
                Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone.Trim(),
                Email = string.IsNullOrWhiteSpace(Email) ? null : Email.Trim()
            };

            // В зависимости от режима — UPDATE или CREATE
            if (IsEditMode && _entityToUpdate != null)
                await _counterpartiesClient.UpdateAsync(_entityToUpdate.Id, dto);
            else
                await _counterpartiesClient.CreateAsync(dto);

            // Закрываем попап. Shell.Current может быть null (например, в unit-тестах) —
            // защищаемся от NullReferenceException
            if (Shell.Current != null)
                await _popupService.ClosePopupAsync(Shell.Current.Navigation);
        }
        catch (Exception ex)
        {
            // Показываем текст ошибки в UI. ApiException (NSwag) содержит код и тело ответа
            ErrorMessage = $"Ошибка сохранения: {ex.Message}";
        }
    }

    /// <summary>
    /// Срабатывает при изменении <see cref="ErrorMessage"/>.
    /// </summary>
    /// <param name="value">Новое значение сообщения.</param>
    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));
}
