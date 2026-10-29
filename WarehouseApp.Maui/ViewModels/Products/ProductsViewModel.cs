using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WarehouseApp.Application.Dtos.CategoryDtos;
using WarehouseApp.Application.Dtos.ProductDtos;
using WarehouseApp.Application.Services;

namespace WarehouseApp.Maui.ViewModels.Products;

/// <summary>
/// ViewModel страницы списка товаров. Управляет загрузкой, поиском и фильтрацией.
/// </summary>
public partial class ProductsViewModel : ObservableObject
{
    private const string StockFilterDefault = "По умолчанию";
    private const string StockFilterInStock = "В наличии";
    private const string StockFilterOutOfStock = "Нет в наличии";

    private readonly IProductApiService _productService;
    private readonly ILookupApiService _lookupService;
    private readonly ILogger<ProductsViewModel> _logger;

    private List<ProductDto> _allProducts = new();
    private bool _isLoading;

    /// <summary>Текст поиска (по названию или артикулу).</summary>
    [ObservableProperty] private string searchText = string.Empty;

    /// <summary>Флаг «идёт обновление» (для RefreshView).</summary>
    [ObservableProperty] private bool isRefreshing;

    /// <summary>Выбранная категория для фильтра.</summary>
    [ObservableProperty] private CategoryDto? selectedFilterCategory;

    /// <summary>Выбранный фильтр по остатку (одна из констант StockFilter*).</summary>
    [ObservableProperty] private string? selectedStockFilter;

    /// <summary>Флаг видимости панели фильтров.</summary>
    [ObservableProperty] private bool isFilterVisible;

    /// <summary>Категории для выпадающего списка фильтра.</summary>
    public ObservableCollection<CategoryDto> AvailableCategories { get; } = new();

    /// <summary>Варианты фильтра по остатку.</summary>
    public List<string> StockOptions { get; } = new()
    {
        StockFilterDefault, StockFilterInStock, StockFilterOutOfStock
    };

    /// <summary>Отфильтрованный список товаров, привязанный к UI.</summary>
    public ObservableCollection<ProductDto> Products { get; } = new();

    /// <summary>
    /// Событие запроса открытия формы товара. Параметр — DTO для редактирования
    /// или <c>null</c> для создания.
    /// </summary>
    public event Func<ProductDto?, Task>? OpenProductFormRequested;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="ProductsViewModel"/>.
    /// </summary>
    public ProductsViewModel(
        IProductApiService productService,
        ILookupApiService lookupService,
        ILogger<ProductsViewModel> logger)
    {
        _productService = productService;
        _lookupService = lookupService;
        _logger = logger;
    }

    /// <summary>
    /// Загружает товары и категории с сервера параллельно и применяет фильтры.
    /// </summary>
    [RelayCommand]
    public async Task LoadDataAsync()
    {
        if (_isLoading) return;
        _isLoading = true;
        IsRefreshing = true;

        try
        {
            var productsTask = _productService.GetAllAsync();
            var categoriesTask = _lookupService.GetCategoriesAsync();
            await Task.WhenAll(productsTask, categoriesTask);

            _allProducts = await productsTask;

            var cats = await categoriesTask;
            AvailableCategories.Clear();
            foreach (var cat in cats) AvailableCategories.Add(cat);

            ApplyFilters();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки товаров");
            _allProducts = new();
            ApplyFilters();
        }
        finally
        {
            IsRefreshing = false;
            _isLoading = false;
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnSelectedFilterCategoryChanged(CategoryDto? value) => ApplyFilters();
    partial void OnSelectedStockFilterChanged(string? value) => ApplyFilters();

    /// <summary>Переключает видимость панели фильтров.</summary>
    [RelayCommand]
    private void ToggleFilter() => IsFilterVisible = !IsFilterVisible;

    /// <summary>
    /// Применяет поиск и фильтры к исходному списку товаров.
    /// </summary>
    [RelayCommand]
    public void ApplyFilters()
    {
        var filtered = _allProducts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
            filtered = filtered.Where(p =>
                p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                p.Article.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        if (SelectedFilterCategory != null)
            filtered = filtered.Where(p => p.CategoryId == SelectedFilterCategory.Id);

        if (SelectedStockFilter == StockFilterInStock)
            filtered = filtered.Where(p => p.Quantity > 0);
        else if (SelectedStockFilter == StockFilterOutOfStock)
            filtered = filtered.Where(p => p.Quantity == 0);

        Products.Clear();
        foreach (var item in filtered) Products.Add(item);
    }

    /// <summary>
    /// Сбрасывает все фильтры и применяет фильтрацию повторно.
    /// </summary>
    [RelayCommand]
    public void ResetFilters()
    {
        SearchText = string.Empty;
        SelectedFilterCategory = null;
        SelectedStockFilter = null;
        ApplyFilters();
    }

    /// <summary>
    /// Команда открытия формы товара.
    /// </summary>
    /// <param name="product"><c>null</c> — создание, DTO — редактирование.</param>
    [RelayCommand]
    private async Task Edit(ProductDto? product)
    {
        if (OpenProductFormRequested != null)
            await OpenProductFormRequested.Invoke(product);
    }
}
