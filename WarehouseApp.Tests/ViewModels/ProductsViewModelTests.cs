using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WarehouseApp.Application.Dtos.CategoryDtos;
using WarehouseApp.Application.Dtos.ProductDtos;
using WarehouseApp.Infrastructure.Client.Generated;
using WarehouseApp.Maui.ViewModels.Products;

namespace WarehouseApp.Tests.ViewModels;

/// <summary>
/// Unit-тесты <see cref="ProductsViewModel"/>.
/// </summary>
public class ProductsViewModelTests
{
    private readonly IProductsClient _productsClient;
    private readonly ILookupsClient _lookupsClient;
    private readonly ILogger<ProductsViewModel> _logger;

    /// <summary>Инициализирует моки.</summary>
    public ProductsViewModelTests()
    {
        _productsClient = Substitute.For<IProductsClient>();
        _lookupsClient = Substitute.For<ILookupsClient>();
        _logger = Substitute.For<ILogger<ProductsViewModel>>();
    }

    /// <summary>
    /// Готовит моки так, чтобы загрузка завершилась успешно.
    /// Мокаются <b>беспараметрические</b> перегрузки — их вызывает VM.
    /// </summary>
    private void Setup_Load_Success(List<ProductDto>? products = null, List<CategoryDto>? categories = null)
    {
        _productsClient.GetAllAsync()
            .Returns(Task.FromResult<ICollection<ProductDto>>(
                products ?? new List<ProductDto>()));
        _lookupsClient.GetCategoriesAsync()
            .Returns(Task.FromResult<ICollection<CategoryDto>>(
                categories ?? new List<CategoryDto>()));
    }

    /// <summary>LoadDataAsync загружает товары и категории, снимает IsRefreshing.</summary>
    [Fact]
    public async Task LoadDataAsync_Populates_Products_And_Categories()
    {
        Setup_Load_Success(
            new List<ProductDto> { new() { Id = 1, Name = "Товар" } },
            new List<CategoryDto> { new() { Id = 1, Name = "Кат" } });

        var vm = new ProductsViewModel(_productsClient, _lookupsClient, _logger);
        await vm.LoadDataAsync();

        vm.Products.Should().HaveCount(1);
        vm.AvailableCategories.Should().HaveCount(1);
        vm.IsRefreshing.Should().BeFalse();
    }

    /// <summary>LoadDataAsync при ошибке очищает Products и снимает IsRefreshing.</summary>
    [Fact]
    public async Task LoadDataAsync_On_Error_Clears_And_Stops_Loading()
    {
        _productsClient.GetAllAsync()
            .Returns(Task.FromException<ICollection<ProductDto>>(
                new HttpRequestException("network")));
        _lookupsClient.GetCategoriesAsync()
            .Returns(Task.FromResult<ICollection<CategoryDto>>(new List<CategoryDto>()));

        var vm = new ProductsViewModel(_productsClient, _lookupsClient, _logger);
        await vm.LoadDataAsync();

        vm.Products.Should().BeEmpty();
        vm.IsRefreshing.Should().BeFalse();
    }

    /// <summary>Поиск по имени/артикулу фильтрует Products.</summary>
    [Fact]
    public async Task ApplyFilters_Filters_By_Name_Or_Article()
    {
        Setup_Load_Success(new List<ProductDto>
        {
            new() { Name = "Молоко", Article = "M-01" },
            new() { Name = "Хлеб", Article = "H-01" }
        });
        var vm = new ProductsViewModel(_productsClient, _lookupsClient, _logger);
        await vm.LoadDataAsync();

        vm.SearchText = "Молок";
        vm.Products.Should().HaveCount(1);
        vm.Products[0].Name.Should().Be("Молоко");

        vm.SearchText = "H-01";     // поиск по артикулу
        vm.Products.Should().HaveCount(1);
        vm.Products[0].Name.Should().Be("Хлеб");
    }

    /// <summary>Фильтр по категории оставляет только товары этой категории.</summary>
    [Fact]
    public async Task ApplyFilters_Filters_By_Category()
    {
        Setup_Load_Success(
            new List<ProductDto>
            {
                new() { Name = "A", CategoryId = 1 },
                new() { Name = "B", CategoryId = 2 }
            },
            new List<CategoryDto>
            {
                new() { Id = 1, Name = "Кат1" },
                new() { Id = 2, Name = "Кат2" }
            });

        var vm = new ProductsViewModel(_productsClient, _lookupsClient, _logger);
        await vm.LoadDataAsync();

        vm.SelectedFilterCategory = vm.AvailableCategories.First(c => c.Id == 2);

        vm.Products.Should().HaveCount(1);
        vm.Products[0].Name.Should().Be("B");
    }

    /// <summary>Фильтр «В наличии» оставляет товары с Quantity &gt; 0.</summary>
    [Fact]
    public async Task ApplyFilters_By_InStock_Leaves_Positive_Quantity()
    {
        Setup_Load_Success(new List<ProductDto>
        {
            new() { Name = "A", Quantity = 5m },
            new() { Name = "B", Quantity = 0m }
        });

        var vm = new ProductsViewModel(_productsClient, _lookupsClient, _logger);
        await vm.LoadDataAsync();

        vm.SelectedStockFilter = "В наличии";

        vm.Products.Should().HaveCount(1);
        vm.Products[0].Name.Should().Be("A");
    }

    /// <summary>Фильтр «Нет в наличии» оставляет товары с Quantity = 0.</summary>
    [Fact]
    public async Task ApplyFilters_By_OutOfStock_Leaves_Zero_Quantity()
    {
        Setup_Load_Success(new List<ProductDto>
        {
            new() { Name = "A", Quantity = 5m },
            new() { Name = "B", Quantity = 0m }
        });

        var vm = new ProductsViewModel(_productsClient, _lookupsClient, _logger);
        await vm.LoadDataAsync();

        vm.SelectedStockFilter = "Нет в наличии";

        vm.Products.Should().HaveCount(1);
        vm.Products[0].Name.Should().Be("B");
    }

    /// <summary>ResetFilters сбрасывает все фильтры и показывает полный список.</summary>
    [Fact]
    public async Task ResetFilters_Clears_All_Filters()
    {
        Setup_Load_Success(new List<ProductDto>
        {
            new() { Name = "A", CategoryId = 1, Quantity = 5m },
            new() { Name = "B", CategoryId = 2, Quantity = 0m }
        });

        var vm = new ProductsViewModel(_productsClient, _lookupsClient, _logger);
        await vm.LoadDataAsync();

        vm.SearchText = "A";
        vm.SelectedStockFilter = "В наличии";

        vm.ResetFilters();

        vm.SearchText.Should().BeEmpty();
        vm.SelectedFilterCategory.Should().BeNull();
        vm.SelectedStockFilter.Should().BeNull();
        vm.Products.Should().HaveCount(2);
    }

    /// <summary>ToggleFilter меняет видимость панели фильтров.</summary>
    [Fact]
    public void ToggleFilter_Changes_IsFilterVisible()
    {
        var vm = new ProductsViewModel(_productsClient, _lookupsClient, _logger);
        vm.IsFilterVisible.Should().BeFalse();

        vm.ToggleFilterCommand.Execute(null);
        vm.IsFilterVisible.Should().BeTrue();

        vm.ToggleFilterCommand.Execute(null);
        vm.IsFilterVisible.Should().BeFalse();
    }

    /// <summary>Edit поднимает событие OpenProductFormRequested с переданным DTO.</summary>
    [Fact]
    public async Task Edit_Raises_OpenProductFormRequested()
    {
        var vm = new ProductsViewModel(_productsClient, _lookupsClient, _logger);
        ProductDto? captured = null;
        vm.OpenProductFormRequested += p => { captured = p; return Task.CompletedTask; };

        var product = new ProductDto { Id = 1, Name = "Товар" };
        await vm.EditCommand.ExecuteAsync(product);

        captured.Should().BeSameAs(product);
    }
}
