using FluentAssertions;
using NSubstitute;
using WarehouseApp.Application.Services;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Maui.ViewModels.Products;

namespace WarehouseApp.Tests.ViewModels;

/// <summary>
/// Тесты для <see cref="ProductsViewModel"/>. 
/// Проверяют фильтрацию, поиск и загрузку данных.
/// </summary>
public class ProductsViewModelTests
{
    private readonly IProductApiService _productServiceMock;
    private readonly ILookupApiService _lookupServiceMock;

    public ProductsViewModelTests()
    {
        _productServiceMock = Substitute.For<IProductApiService>();
        _lookupServiceMock = Substitute.For<ILookupApiService>();
    }

    [Fact]
    public async Task LoadDataAsync_Should_Populate_Products_And_Categories()
    {
        // Arrange
        var fakeProducts = new List<Product> { new() { Name = "Товар 1" } };
        var fakeCategories = new List<Category> { new() { Name = "Категория 1" } };

        _productServiceMock.GetAllAsync(Arg.Any<CancellationToken>()).Returns(fakeProducts);
        _lookupServiceMock.GetCategoriesAsync().Returns(fakeCategories);

        var vm = new ProductsViewModel(_productServiceMock, _lookupServiceMock);

        // Act
        await vm.LoadDataAsync();

        // Assert
        vm.Products.Should().HaveCount(1);
        vm.AvailableCategories.Should().HaveCount(1);
        vm.IsRefreshing.Should().BeFalse();
    }

    [Fact]
    public async Task ApplyFilters_Should_Filter_By_SearchText()
    {
        // Arrange
        var fakeProducts = new List<Product>
        {
            new() { Name = "Молоко", Article = "M-01" },
            new() { Name = "Хлеб", Article = "H-01" }
        };
        _productServiceMock.GetAllAsync(Arg.Any<CancellationToken>()).Returns(fakeProducts);
        _lookupServiceMock.GetCategoriesAsync().Returns(new List<Category>());

        var vm = new ProductsViewModel(_productServiceMock, _lookupServiceMock);
        await vm.LoadDataAsync();

        // Act
        vm.SearchText = "Молок"; // Имитируем ввод

        // Assert
        vm.Products.Should().HaveCount(1);
        vm.Products.First().Name.Should().Be("Молоко");
    }

    [Fact]
    public void ToggleFilter_Should_Change_Visibility()
    {
        // Arrange
        var vm = new ProductsViewModel(_productServiceMock, _lookupServiceMock);
        vm.IsFilterVisible.Should().BeFalse();

        // Act
        vm.ToggleFilterCommand.Execute(null);

        // Assert
        vm.IsFilterVisible.Should().BeTrue();
    }
}
