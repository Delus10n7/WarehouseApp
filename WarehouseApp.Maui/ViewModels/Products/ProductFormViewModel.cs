using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Application.Services;

namespace WarehouseApp.Maui.ViewModels.Products;

/// <summary>
/// ViewModel для формы создания и редактирования товара.
/// </summary>
public partial class ProductFormViewModel : ObservableObject
{
    private readonly IPopupService _popupService;
    private readonly IProductApiService _productService;
    private readonly ILookupApiService _lookupService;
    private Product? _entityToUpdate;

    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string buttonText = string.Empty;
    [ObservableProperty] private Color buttonColor = Colors.Transparent;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string article = string.Empty;
    [ObservableProperty] private Category? selectedCategory;
    [ObservableProperty] private UnitOfMeasure? selectedUnit;
    [ObservableProperty] private string errorMessage = string.Empty;

    public ObservableCollection<Category> Categories { get; } = new();
    public ObservableCollection<UnitOfMeasure> Units { get; } = new();

    /// <summary>Событие для запроса открытия вложенного попапа (справочника).</summary>
    public Func<LookupPopupViewModel, Task>? ShowLookupPopupRequested { get; set; }

    public bool IsEditMode { get; }
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    // Конструктор для создания
    public ProductFormViewModel(IPopupService popupService, IProductApiService productService, ILookupApiService lookupService)
    {
        _popupService = popupService;
        _productService = productService;
        _lookupService = lookupService;

        IsEditMode = false;
        Title = "Создание товара";
        ButtonText = "Добавить товар";
        ButtonColor = Color.FromArgb("#4CAF50");

        _ = LoadLookupsAsync();
    }

    // Конструктор для редактирования
    public ProductFormViewModel(IPopupService popupService, IProductApiService productService, ILookupApiService lookupService, Product existingProduct)
    {
        _popupService = popupService;
        _productService = productService;
        _lookupService = lookupService;
        _entityToUpdate = existingProduct;

        IsEditMode = true;
        Title = "Изменение товара";
        ButtonText = "Изменить товар";
        ButtonColor = Color.FromArgb("#1976D2");

        Name = existingProduct.Name;
        Article = existingProduct.Article;

        _ = LoadLookupsAsync();
    }

    /// <summary>
    /// Загружает списки категорий и единиц измерения.
    /// </summary>
    public async Task LoadLookupsAsync()
    {
        try
        {
            var cats = await _lookupService.GetCategoriesAsync();
            var units = await _lookupService.GetUnitsAsync();

            Categories.Clear(); foreach (var c in cats) Categories.Add(c);
            Units.Clear(); foreach (var u in units) Units.Add(u);

            if (IsEditMode && _entityToUpdate != null)
            {
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == _entityToUpdate.CategoryId);
                SelectedUnit = Units.FirstOrDefault(u => u.Id == _entityToUpdate.UnitOfMeasureId);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ошибка загрузки справочников: " + ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
    }

    /// <summary>
    /// Сохраняет товар (создает новый или обновляет существующий).
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        var missingFields = new List<string>();

        if (string.IsNullOrWhiteSpace(Name)) missingFields.Add("Название товара");
        if (string.IsNullOrWhiteSpace(Article)) missingFields.Add("Артикул");
        if (SelectedCategory == null) missingFields.Add("Категория");
        if (SelectedUnit == null) missingFields.Add("Единица измерения");

        // Логика формирования ошибки
        if (missingFields.Count == 1)
        {
            // Если пустое только одно поле — пишем какое именно
            ErrorMessage = $"Поле «{missingFields[0]}» обязательно для заполнения.";
            OnPropertyChanged(nameof(HasError));
            return;
        }
        else if (missingFields.Count > 1)
        {
            // Если пустых полей несколько — выводим общее сообщение
            ErrorMessage = "Заполните все обязательные поля.";
            OnPropertyChanged(nameof(HasError));
            return;
        }

        // Если ошибок нет, продолжаем сохранение
        ErrorMessage = string.Empty;
        OnPropertyChanged(nameof(HasError));

        try
        {
            if (IsEditMode && _entityToUpdate != null)
            {
                _entityToUpdate.Name = Name.Trim();
                _entityToUpdate.Article = Article.Trim();
                _entityToUpdate.CategoryId = SelectedCategory!.Id;
                _entityToUpdate.UnitOfMeasureId = SelectedUnit!.Id;
                await _productService.UpdateAsync(_entityToUpdate);
            }
            else
            {
                var newProduct = new Product
                {
                    Name = Name.Trim(),
                    Article = Article.Trim(),
                    CategoryId = SelectedCategory!.Id,
                    UnitOfMeasureId = SelectedUnit!.Id,
                    Quantity = 0,
                    PurchasePrice = 0,
                    SalePrice = 0
                };
                await _productService.AddAsync(newProduct);
            }

            if (Shell.Current != null)
            {
                await _popupService.ClosePopupAsync(Shell.Current.Navigation);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
    }

    partial void OnErrorMessageChanged(string value) => OnPropertyChanged(nameof(HasError));

    /// <summary>
    /// Открывает окно добавления новой категории.
    /// </summary>
    [RelayCommand]
    private async Task AddNewCategoryAsync()
    {
        var vm = new LookupPopupViewModel("Создание категории товара", "Введите название категории...", "Добавить категорию");
        vm.OnSave = async (name) =>
        {
            var newCat = await _lookupService.AddCategoryAsync(new Category { Name = name });
            Categories.Add(newCat);
            SelectedCategory = newCat;
        };
        if (ShowLookupPopupRequested != null) await ShowLookupPopupRequested(vm);
    }

    /// <summary>
    /// Открывает окно добавления новой единицы измерения.
    /// </summary>
    [RelayCommand]
    private async Task AddNewUnitAsync()
    {
        var vm = new LookupPopupViewModel("Создание ед. измерения", "Введите наименование...", "Добавить единицу измерения");
        vm.OnSave = async (name) =>
        {
            var newUnit = await _lookupService.AddUnitAsync(new UnitOfMeasure { Name = name });
            Units.Add(newUnit);
            SelectedUnit = newUnit;
        };
        if (ShowLookupPopupRequested != null) await ShowLookupPopupRequested(vm);
    }
}
