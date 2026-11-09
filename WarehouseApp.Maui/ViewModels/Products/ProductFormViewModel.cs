using System.Collections.ObjectModel;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WarehouseApp.Application.Dtos.CategoryDtos;
using WarehouseApp.Application.Dtos.ProductDtos;
using WarehouseApp.Application.Dtos.UnitOfMeasureDtos;
using WarehouseApp.Infrastructure.Client.Generated;

namespace WarehouseApp.Maui.ViewModels.Products;

/// <summary>
/// ViewModel модальной формы создания и редактирования товара.
/// </summary>
public partial class ProductFormViewModel : ObservableObject
{
    /// <summary>Сервис управления попапами.</summary>
    private readonly IPopupService _popupService;

    /// <summary>Сгенерированный клиент API товаров.</summary>
    private readonly IProductsClient _productsClient;

    /// <summary>Сгенерированный клиент API справочников.</summary>
    private readonly ILookupsClient _lookupsClient;

    /// <summary>DTO редактируемого товара. <c>null</c> в режиме создания.</summary>
    private readonly ProductDto? _entityToUpdate;

    /// <summary>Заголовок окна («Создание товара» / «Изменение товара»).</summary>
    [ObservableProperty] private string title = string.Empty;

    /// <summary>Текст на кнопке сохранения.</summary>
    [ObservableProperty] private string buttonText = string.Empty;

    /// <summary>Цвет кнопки сохранения.</summary>
    [ObservableProperty] private Color buttonColor = Colors.Transparent;

    /// <summary>Название товара.</summary>
    [ObservableProperty] private string name = string.Empty;

    /// <summary>Артикул товара.</summary>
    [ObservableProperty] private string article = string.Empty;

    /// <summary>Выбранная категория (привязана к <c>Picker.SelectedItem</c>).</summary>
    [ObservableProperty] private CategoryDto? selectedCategory;

    /// <summary>Выбранная единица измерения.</summary>
    [ObservableProperty] private UnitOfMeasureDto? selectedUnit;

    /// <summary>Текст ошибки — привязан к <c>Label.Text</c>.</summary>
    [ObservableProperty] private string errorMessage = string.Empty;

    /// <summary>Список категорий для выпадающего списка.</summary>
    /// <remarks>Заполняется в <see cref="InitializeAsync"/>.</remarks>
    public ObservableCollection<CategoryDto> Categories { get; } = new();

    /// <summary>Список единиц измерения для выпадающего списка.</summary>
    /// <remarks>Заполняется в <see cref="InitializeAsync"/>.</remarks>
    public ObservableCollection<UnitOfMeasureDto> Units { get; } = new();

    /// <summary>
    /// Callback для открытия вложенного попапа (создание категории/единицы).
    /// </summary>
    public Func<LookupPopupViewModel, Task>? ShowLookupPopupRequested { get; set; }

    /// <summary>Признак режима редактирования.</summary>
    public bool IsEditMode => _entityToUpdate != null;

    /// <summary>Признак наличия ошибки для <c>Label.IsVisible</c>.</summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Конструктор для режима создания нового товара.
    /// </summary>
    /// <param name="popupService">Сервис попапов.</param>
    /// <param name="productsClient">Клиент API товаров.</param>
    /// <param name="lookupsClient">Клиент API справочников.</param>
    public ProductFormViewModel(
        IPopupService popupService,
        IProductsClient productsClient,
        ILookupsClient lookupsClient)
    {
        _popupService = popupService;
        _productsClient = productsClient;
        _lookupsClient = lookupsClient;

        Title = "Создание товара";
        ButtonText = "Добавить товар";
        ButtonColor = Color.FromArgb("#4CAF50");
    }

    /// <summary>
    /// Конструктор для режима редактирования существующего товара.
    /// </summary>
    /// <param name="popupService">Сервис попапов.</param>
    /// <param name="productsClient">Клиент API товаров.</param>
    /// <param name="lookupsClient">Клиент API справочников.</param>
    /// <param name="existing">DTO существующего товара.</param>
    public ProductFormViewModel(
        IPopupService popupService,
        IProductsClient productsClient,
        ILookupsClient lookupsClient,
        ProductDto existing)
    {
        _popupService = popupService;
        _productsClient = productsClient;
        _lookupsClient = lookupsClient;
        _entityToUpdate = existing;

        Title = "Изменение товара";
        ButtonText = "Изменить товар";
        ButtonColor = Color.FromArgb("#1976D2");

        // Заполняем редактируемые поля
        Name = existing.Name;
        Article = existing.Article;
        // CategoryId/UnitOfMeasureId восстановим в InitializeAsync после загрузки справочников
    }

    /// <summary>
    /// Асинхронная инициализация формы: загрузка справочников и восстановление выбранных значений.
    /// </summary>
    /// <returns>Задача, завершающаяся после загрузки.</returns>
    public async Task InitializeAsync()
    {
        try
        {
            // Параллельная загрузка — оба запроса стартуют одновременно
            var catsTask = _lookupsClient.GetCategoriesAsync();
            var unitsTask = _lookupsClient.GetUnitsAsync();
            await Task.WhenAll(catsTask, unitsTask);

            // Дожидаемся результатов (они уже завершены, await мгновенный)
            var cats = await catsTask;
            var units = await unitsTask;

            // Переносим категории в ObservableCollection → Picker обновится
            Categories.Clear();
            foreach (var c in cats) Categories.Add(c);

            // Переносим единицы измерения
            Units.Clear();
            foreach (var u in units) Units.Add(u);

            // Режим редактирования: восстанавливаем выбранные элементы по Id
            if (IsEditMode && _entityToUpdate != null)
            {
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == _entityToUpdate.CategoryId);
                SelectedUnit = Units.FirstOrDefault(u => u.Id == _entityToUpdate.UnitOfMeasureId);
            }
        }
        catch (Exception ex)
        {
            // Показываем ошибку в форме — пользователь увидит, что справочники не загрузились
            ErrorMessage = "Ошибка загрузки справочников: " + ex.Message;
        }
    }

    /// <summary>
    /// Валидирует форму и сохраняет товар (создание или обновление).
    /// </summary>
    /// <returns>Задача, завершающаяся после сохранения или показа ошибки.</returns>
    [RelayCommand]
    private async Task SaveAsync()
    {
        // Собираем список пустых обязательных полей
        var missingFields = new List<string>();

        if (string.IsNullOrWhiteSpace(Name)) missingFields.Add("Название товара");
        if (string.IsNullOrWhiteSpace(Article)) missingFields.Add("Артикул");
        if (SelectedCategory == null) missingFields.Add("Категория");
        if (SelectedUnit == null) missingFields.Add("Единица измерения");

        // Одно пустое поле — конкретное сообщение, какое именно
        if (missingFields.Count == 1)
        {
            ErrorMessage = $"Поле «{missingFields[0]}» обязательно для заполнения.";
            return;
        }

        // Несколько пустых — общее сообщение
        if (missingFields.Count > 1)
        {
            ErrorMessage = "Заполните все обязательные поля.";
            return;
        }

        // Всё заполнено — сбрасываем ошибку
        ErrorMessage = string.Empty;

        try
        {
            // Готовим DTO (общий для POST и PUT)
            var dto = new ProductRequestDto
            {
                Name = Name.Trim(),
                Article = Article.Trim(),
                CategoryId = SelectedCategory!.Id,       // ! — гарантировано проверкой выше
                UnitOfMeasureId = SelectedUnit!.Id
            };

            // Update или Create в зависимости от режима
            if (IsEditMode && _entityToUpdate != null)
                await _productsClient.UpdateAsync(_entityToUpdate.Id, dto);
            else
                await _productsClient.CreateAsync(dto);

            // Закрываем попап (защита от null в unit-тестах)
            if (Shell.Current != null)
                await _popupService.ClosePopupAsync(Shell.Current.Navigation);
        }
        catch (Exception ex)
        {
            // ApiException (NSwag) содержит код и тело ответа
            ErrorMessage = ex.Message;
        }
    }

    /// <summary>
    /// Срабатывает при изменении <see cref="ErrorMessage"/> — уведомляет UI об изменении <see cref="HasError"/>.
    /// </summary>
    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));

    /// <summary>
    /// Открывает диалог создания новой категории и добавляет её в список.
    /// </summary>
    /// <returns>Задача, завершающаяся после закрытия вложенного попапа.</returns>
    [RelayCommand]
    private async Task AddNewCategoryAsync()
    {
        // Создаём ViewModel вложенного попапа с нужными текстами
        var vm = new LookupPopupViewModel(
            "Создание категории товара",
            "Введите название категории...",
            "Добавить категорию");

        // Callback: что делать при вводе имени и нажатии «Сохранить»
        vm.OnSave = async (name) =>
        {
            // Вызываем API — сервер создаст категорию и вернёт её с присвоенным Id
            var created = await _lookupsClient.CreateCategoryAsync(new CategoryCreateDto { Name = name });

            // Добавляем в ObservableCollection — Picker тут же увидит новую категорию
            Categories.Add(created);

            // Сразу выбираем её — пользователю не нужно открывать Picker повторно
            SelectedCategory = created;
        };

        // Показываем попап через callback (code-behind знает, как это делать)
        if (ShowLookupPopupRequested != null)
            await ShowLookupPopupRequested(vm);
    }

    /// <summary>
    /// Открывает диалог создания новой единицы измерения и добавляет её в список.
    /// </summary>
    /// <returns>Задача, завершающаяся после закрытия вложенного попапа.</returns>
    [RelayCommand]
    private async Task AddNewUnitAsync()
    {
        var vm = new LookupPopupViewModel(
            "Создание ед. измерения",
            "Введите наименование...",
            "Добавить единицу измерения");

        vm.OnSave = async (name) =>
        {
            var created = await _lookupsClient.CreateUnitAsync(new UnitOfMeasureCreateDto { Name = name });
            Units.Add(created);
            SelectedUnit = created;
        };

        if (ShowLookupPopupRequested != null)
            await ShowLookupPopupRequested(vm);
    }
}
