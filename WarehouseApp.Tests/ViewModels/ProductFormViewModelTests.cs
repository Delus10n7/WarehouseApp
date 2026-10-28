using CommunityToolkit.Maui;
using FluentAssertions;
using NSubstitute;
using WarehouseApp.Application.Services;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Maui.ViewModels.Products;

namespace WarehouseApp.Tests.ViewModels;

/// <summary>
/// Тесты для <see cref="ProductFormViewModel"/>. 
/// Проверяют валидацию формы и сохранение данных.
/// </summary>
public class ProductFormViewModelTests
{
    private readonly IPopupService _popupServiceMock;
    private readonly IProductApiService _productServiceMock;
    private readonly ILookupApiService _lookupServiceMock;

    public ProductFormViewModelTests()
    {
        _popupServiceMock = Substitute.For<IPopupService>();
        _productServiceMock = Substitute.For<IProductApiService>();
        _lookupServiceMock = Substitute.For<ILookupApiService>();
    }

    [Fact]
    public async Task SaveAsync_Should_Set_Specific_Error_When_One_Field_Is_Empty()
    {
        // Arrange
        var vm = new ProductFormViewModel(_popupServiceMock, _productServiceMock, _lookupServiceMock);

        // Заполняем все, кроме Артикула
        vm.Name = "Товар";
        vm.SelectedCategory = new Category { Id = 1 };
        vm.SelectedUnit = new UnitOfMeasure { Id = 1 };
        vm.Article = "";

        // Act
        await vm.SaveCommand.ExecuteAsync(null);

        // Assert
        vm.HasError.Should().BeTrue();
        vm.ErrorMessage.Should().Contain("Артикул"); // Проверяем, что указано конкретное поле
        await _productServiceMock.DidNotReceive().AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveAsync_Should_Set_General_Error_When_Multiple_Fields_Are_Empty()
    {
        // Arrange
        var vm = new ProductFormViewModel(_popupServiceMock, _productServiceMock, _lookupServiceMock);
        // Оставляем всё пустым

        // Act
        await vm.SaveCommand.ExecuteAsync(null);

        // Assert
        vm.HasError.Should().BeTrue();
        vm.ErrorMessage.Should().Be("Заполните все обязательные поля.");
    }

    [Fact]
    public async Task SaveAsync_Should_Call_AddAsync_When_Valid()
    {
        // Arrange
        var vm = new ProductFormViewModel(_popupServiceMock, _productServiceMock, _lookupServiceMock);
        vm.Name = "Новый товар";
        vm.Article = "NEW-01";
        vm.SelectedCategory = new Category { Id = 1 };
        vm.SelectedUnit = new UnitOfMeasure { Id = 1 };

        // Act
        await vm.SaveCommand.ExecuteAsync(null);

        // Assert
        vm.HasError.Should().BeFalse();
        await _productServiceMock.Received(1).AddAsync(
            Arg.Is<Product>(p => p.Name == "Новый товар" && p.Article == "NEW-01"),
            Arg.Any<CancellationToken>());
    }
}
