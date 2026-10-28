using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Application.Services;

namespace WarehouseApp.Maui.ViewModels.Products;

/// <summary>
/// ViewModel для страницы списка товаров. Управляет загрузкой, поиском и фильтрацией.
/// </summary>
public partial class ProductsViewModel : ObservableObject
{
    private readonly IProductApiService _productService;
    private readonly ILookupApiService _lookupService;
    private List<Product> _allProducts = new();

    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private Category? selectedFilterCategory;
    [ObservableProperty] private string? selectedStockFilter;
    [ObservableProperty] private bool isFilterVisible;
    private bool _isLoading;

    public ObservableCollection<Category> AvailableCategories { get; set; } = new();
    public List<string> StockOptions { get; } = new() { "По умолчанию", "В наличии", "Нет в наличии" };

    public ObservableCollection<Product> Products { get; } = new();

    public event Func<Product?, Task>? OpenProductFormRequested;

    public ProductsViewModel(IProductApiService productService, ILookupApiService lookupService)
    {
        _productService = productService;
        _lookupService = lookupService;
    }

    /// <summary>
    /// Загружает список товаров с сервера.
    /// </summary>
    [RelayCommand]
    public async Task LoadDataAsync()
    {
        if (_isLoading) return;
        _isLoading = true;
        IsRefreshing = true;
        try
        {
            _allProducts = await _productService.GetAllAsync();
            ApplyFilters();

            // Загружаем категории для фильтра (если они еще не загружены)
            AvailableCategories.Clear();
            var cats = await _lookupService.GetCategoriesAsync();
            foreach (var cat in cats) AvailableCategories.Add(cat);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка API: {ex.Message}");
            _allProducts = new List<Product>();
            ApplyFilters();
        }
        finally { IsRefreshing = false; _isLoading = false; }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnSelectedFilterCategoryChanged(Category? value) => ApplyFilters();
    partial void OnSelectedStockFilterChanged(string? value) => ApplyFilters();

    /// <summary>Переключает видимость панели фильтра.</summary>
    [RelayCommand]
    private void ToggleFilter() => IsFilterVisible = !IsFilterVisible;

    /// <summary>
    /// Применяет поиск и фильтры к исходному списку.
    /// </summary>
    [RelayCommand]
    public void ApplyFilters()
    {
        var filtered = _allProducts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
            filtered = filtered.Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                           p.Article.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        if (SelectedFilterCategory != null)
            filtered = filtered.Where(p => p.CategoryId == SelectedFilterCategory.Id);

        if (SelectedStockFilter == "В наличии")
            filtered = filtered.Where(p => p.Quantity > 0);
        else if (SelectedStockFilter == "Нет в наличии")
            filtered = filtered.Where(p => p.Quantity == 0);

        Products.Clear();
        foreach (var item in filtered) Products.Add(item);
    }

    /// <summary>
    /// Сбрасывает все фильтры.
    /// </summary>
    [RelayCommand]
    public void ResetFilters()
    {
        SearchText = string.Empty;
        SelectedFilterCategory = null;
        SelectedStockFilter = null;
    }

    /// <summary>
    /// Вызывает событие открытия формы товара (создание или редактирование).
    /// </summary>
    [RelayCommand]
    private async Task Edit(Product? product)
    {
        if (OpenProductFormRequested != null) await OpenProductFormRequested.Invoke(product);
    }
}
