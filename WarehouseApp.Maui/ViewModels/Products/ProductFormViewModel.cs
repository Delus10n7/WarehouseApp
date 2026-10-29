using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WarehouseApp.Application.Dtos.CategoryDtos;
using WarehouseApp.Application.Dtos.ProductDtos;
using WarehouseApp.Application.Dtos.UnitOfMeasureDtos;
using WarehouseApp.Application.Services;

namespace WarehouseApp.Maui.ViewModels.Products;

/// <summary>
/// ViewModel модальной формы создания и редактирования товара.
/// </summary>
public partial class ProductFormViewModel : ObservableObject
{
    private readonly IPopupService _popupService;
    private readonly IProductApiService _productService;
    private readonly ILookupApiService _lookupService;
    private readonly ProductDto? _entityToUpdate;

    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string buttonText = string.Empty;
    [ObservableProperty] private Color buttonColor = Colors.Transparent;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string article = string.Empty;
    [ObservableProperty] private CategoryDto? selectedCategory;
    [ObservableProperty] private UnitOfMeasureDto? selectedUnit;
    [ObservableProperty] private string errorMessage = string.Empty;

    /// <summary>Доступные категории (заполняются при <see cref="InitializeAsync"/>).</summary>
    public ObservableCollection<CategoryDto> Categories { get; } = new();

    /// <summary>Доступные единицы измерения.</summary>
    public ObservableCollection<UnitOfMeasureDto> Units { get; } = new();

    /// <summary>
    /// Callback для открытия вложенного попапа справочника (категории / единицы).
    /// Устанавливается из code-behind страницы.
    /// </summary>
    public Func<LookupPopupViewModel, Task>? ShowLookupPopupRequested { get; set; }

    /// <summary>Режим редактирования (в отличие от создания).</summary>
    public bool IsEditMode => _entityToUpdate != null;

    /// <summary>Есть ли сообщение об ошибке для отображения в UI.</summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Конструктор для создания нового товара.
    /// </summary>
    public ProductFormViewModel(
        IPopupService popupService,
        IProductApiService productService,
        ILookupApiService lookupService)
    {
        _popupService = popupService;
        _productService = productService;
        _lookupService = lookupService;

        Title = "Создание товара";
        ButtonText = "Добавить товар";
        ButtonColor = Color.FromArgb("#4CAF50");
    }

    /// <summary>
    /// Конструктор для редактирования существующего товара.
    /// </summary>
    public ProductFormViewModel(
        IPopupService popupService,
        IProductApiService productService,
        ILookupApiService lookupService,
        ProductDto existing)
    {
        _popupService = popupService;
        _productService = productService;
        _lookupService = lookupService;
        _entityToUpdate = existing;

        Title = "Изменение товара";
        ButtonText = "Изменить товар";
        ButtonColor = Color.FromArgb("#1976D2");

        Name = existing.Name;
        Article = existing.Article;
    }

    /// <summary>
    /// Асинхронная инициализация: загрузка категорий и единиц, восстановление
    /// выбранных значений при редактировании.
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            var catsTask = _lookupService.GetCategoriesAsync();
            var unitsTask = _lookupService.GetUnitsAsync();
            await Task.WhenAll(catsTask, unitsTask);

            var cats = await catsTask;
            var units = await unitsTask;

            Categories.Clear();
            foreach (var c in cats) Categories.Add(c);

            Units.Clear();
            foreach (var u in units) Units.Add(u);

            if (IsEditMode && _entityToUpdate != null)
            {
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == _entityToUpdate.CategoryId);
                SelectedUnit = Units.FirstOrDefault(u => u.Id == _entityToUpdate.UnitOfMeasureId);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ошибка загрузки справочников: " + ex.Message;
        }
    }

    /// <summary>
    /// Валидирует форму и сохраняет товар (создание или обновление).
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        var missingFields = new List<string>();

        if (string.IsNullOrWhiteSpace(Name)) missingFields.Add("Название товара");
        if (string.IsNullOrWhiteSpace(Article)) missingFields.Add("Артикул");
        if (SelectedCategory == null) missingFields.Add("Категория");
        if (SelectedUnit == null) missingFields.Add("Единица измерения");

        if (missingFields.Count == 1)
        {
            ErrorMessage = $"Поле «{missingFields[0]}» обязательно для заполнения.";
            return;
        }
        if (missingFields.Count > 1)
        {
            ErrorMessage = "Заполните все обязательные поля.";
            return;
        }

        ErrorMessage = string.Empty;

        try
        {
            var dto = new ProductRequestDto
            {
                Name = Name.Trim(),
                Article = Article.Trim(),
                CategoryId = SelectedCategory!.Id,
                UnitOfMeasureId = SelectedUnit!.Id
            };

            if (IsEditMode && _entityToUpdate != null)
                await _productService.UpdateAsync(_entityToUpdate.Id, dto);
            else
                await _productService.AddAsync(dto);

            if (Shell.Current != null)
                await _popupService.ClosePopupAsync(Shell.Current.Navigation);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));

    /// <summary>
    /// Открывает диалог создания новой категории и добавляет её в список.
    /// </summary>
    [RelayCommand]
    private async Task AddNewCategoryAsync()
    {
        var vm = new LookupPopupViewModel(
            "Создание категории товара",
            "Введите название категории...",
            "Добавить категорию");

        vm.OnSave = async (name) =>
        {
            var created = await _lookupService.AddCategoryAsync(new CategoryCreateDto { Name = name });
            Categories.Add(created);
            SelectedCategory = created;
        };

        if (ShowLookupPopupRequested != null)
            await ShowLookupPopupRequested(vm);
    }

    /// <summary>
    /// Открывает диалог создания новой единицы измерения и добавляет её в список.
    /// </summary>
    [RelayCommand]
    private async Task AddNewUnitAsync()
    {
        var vm = new LookupPopupViewModel(
            "Создание ед. измерения",
            "Введите наименование...",
            "Добавить единицу измерения");

        vm.OnSave = async (name) =>
        {
            var created = await _lookupService.AddUnitAsync(new UnitOfMeasureCreateDto { Name = name });
            Units.Add(created);
            SelectedUnit = created;
        };

        if (ShowLookupPopupRequested != null)
            await ShowLookupPopupRequested(vm);
    }
}
