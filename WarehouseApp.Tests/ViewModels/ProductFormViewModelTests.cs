using CommunityToolkit.Maui;
using FluentAssertions;
using NSubstitute;
using WarehouseApp.Application.Dtos.CategoryDtos;
using WarehouseApp.Application.Dtos.ProductDtos;
using WarehouseApp.Application.Dtos.UnitOfMeasureDtos;
using WarehouseApp.Infrastructure.Client.Generated;
using WarehouseApp.Maui.ViewModels.Products;

namespace WarehouseApp.Tests.ViewModels;

/// <summary>
/// Unit-тесты <see cref="ProductFormViewModel"/>.
/// </summary>
public class ProductFormViewModelTests
{
    private readonly IPopupService _popupService;
    private readonly IProductsClient _productsClient;
    private readonly ILookupsClient _lookupsClient;

    /// <summary>Инициализирует моки.</summary>
    public ProductFormViewModelTests()
    {
        _popupService = Substitute.For<IPopupService>();
        _productsClient = Substitute.For<IProductsClient>();
        _lookupsClient = Substitute.For<ILookupsClient>();
    }

    /// <summary>Конструктор создания: IsEditMode = false.</summary>
    [Fact]
    public void Ctor_Create_Sets_IsEditMode_False()
    {
        var vm = new ProductFormViewModel(_popupService, _productsClient, _lookupsClient);

        vm.IsEditMode.Should().BeFalse();
        vm.Title.Should().Be("Создание товара");
    }

    /// <summary>Конструктор редактирования заполняет Name/Article и IsEditMode = true.</summary>
    [Fact]
    public void Ctor_Edit_Populates_Name_And_Article()
    {
        var existing = new ProductDto { Id = 1, Name = "Телефон", Article = "TEL-1" };

        var vm = new ProductFormViewModel(_popupService, _productsClient, _lookupsClient, existing);

        vm.IsEditMode.Should().BeTrue();
        vm.Name.Should().Be("Телефон");
        vm.Article.Should().Be("TEL-1");
    }

    /// <summary>InitializeAsync загружает категории и единицы; при редактировании выбирает нужные.</summary>
    [Fact]
    public async Task InitializeAsync_Loads_Lookups_And_Restores_Edit_Values()
    {
        // Мокаем БЕСПАРАМЕТРИЧЕСКИЕ перегрузки
        _lookupsClient.GetCategoriesAsync()
            .Returns(Task.FromResult<ICollection<CategoryDto>>(new List<CategoryDto>
            {
                new() { Id = 5, Name = "Кат" }
            }));
        _lookupsClient.GetUnitsAsync()
            .Returns(Task.FromResult<ICollection<UnitOfMeasureDto>>(new List<UnitOfMeasureDto>
            {
                new() { Id = 7, Name = "шт" }
            }));

        var existing = new ProductDto { Id = 1, CategoryId = 5, UnitOfMeasureId = 7 };
        var vm = new ProductFormViewModel(_popupService, _productsClient, _lookupsClient, existing);

        await vm.InitializeAsync();

        vm.Categories.Should().HaveCount(1);
        vm.Units.Should().HaveCount(1);
        vm.SelectedCategory!.Id.Should().Be(5);      // восстановлено из existing
        vm.SelectedUnit!.Id.Should().Be(7);
    }

    /// <summary>InitializeAsync при ошибке записывает сообщение в ErrorMessage.</summary>
    [Fact]
    public async Task InitializeAsync_On_Error_Sets_ErrorMessage()
    {
        _lookupsClient.GetCategoriesAsync()
            .Returns(Task.FromException<ICollection<CategoryDto>>(new Exception("boom")));
        _lookupsClient.GetUnitsAsync()
            .Returns(Task.FromResult<ICollection<UnitOfMeasureDto>>(new List<UnitOfMeasureDto>()));

        var vm = new ProductFormViewModel(_popupService, _productsClient, _lookupsClient);

        await vm.InitializeAsync();

        vm.HasError.Should().BeTrue();
        vm.ErrorMessage.Should().Contain("boom");
    }

    /// <summary>Валидация: одно пустое поле — точечное сообщение.</summary>
    [Theory]
    [InlineData("", "A-1", true, true)]
    [InlineData("Товар", "", true, true)]
    [InlineData("Товар", "A-1", false, true)]
    [InlineData("Товар", "A-1", true, false)]
    public async Task Save_Validation_One_Missing_Field(
        string name, string article, bool hasCategory, bool hasUnit)
    {
        var vm = new ProductFormViewModel(_popupService, _productsClient, _lookupsClient)
        {
            Name = name,
            Article = article,
            SelectedCategory = hasCategory ? new CategoryDto { Id = 1 } : null,
            SelectedUnit = hasUnit ? new UnitOfMeasureDto { Id = 1 } : null
        };

        await vm.SaveCommand.ExecuteAsync(null);

        vm.HasError.Should().BeTrue();
        vm.ErrorMessage.Should().StartWith("Поле");
        await _productsClient.DidNotReceive()
            .CreateAsync(Arg.Any<ProductRequestDto>());
    }

    /// <summary>Валидация: несколько пустых — общее сообщение.</summary>
    [Fact]
    public async Task Save_Validation_Multiple_Missing_Fields()
    {
        var vm = new ProductFormViewModel(_popupService, _productsClient, _lookupsClient);
        // все поля пустые

        await vm.SaveCommand.ExecuteAsync(null);

        vm.HasError.Should().BeTrue();
        vm.ErrorMessage.Should().Be("Заполните все обязательные поля.");
    }

    /// <summary>Успешное создание: вызывается CreateAsync с правильным DTO.</summary>
    [Fact]
    public async Task Save_Create_Calls_CreateAsync()
    {
        _productsClient.CreateAsync(Arg.Any<ProductRequestDto>())
            .Returns(Task.FromResult(new ProductDto { Id = 1 }));

        var vm = new ProductFormViewModel(_popupService, _productsClient, _lookupsClient)
        {
            Name = "  Новый  ",
            Article = "  N-1  ",
            SelectedCategory = new CategoryDto { Id = 5 },
            SelectedUnit = new UnitOfMeasureDto { Id = 7 }
        };

        await vm.SaveCommand.ExecuteAsync(null);

        await _productsClient.Received(1).CreateAsync(
            Arg.Is<ProductRequestDto>(d =>
                d.Name == "Новый" && d.Article == "N-1" &&
                d.CategoryId == 5 && d.UnitOfMeasureId == 7));
    }

    /// <summary>Успешное редактирование: вызывается UpdateAsync с id и DTO.</summary>
    [Fact]
    public async Task Save_Edit_Calls_UpdateAsync()
    {
        _productsClient.UpdateAsync(Arg.Any<int>(), Arg.Any<ProductRequestDto>())
            .Returns(Task.CompletedTask);

        var existing = new ProductDto { Id = 42, Name = "Старое", Article = "OLD" };
        var vm = new ProductFormViewModel(_popupService, _productsClient, _lookupsClient, existing)
        {
            Name = "Новое",
            Article = "NEW",
            SelectedCategory = new CategoryDto { Id = 1 },
            SelectedUnit = new UnitOfMeasureDto { Id = 2 }
        };

        await vm.SaveCommand.ExecuteAsync(null);

        await _productsClient.Received(1).UpdateAsync(
            42,
            Arg.Is<ProductRequestDto>(d => d.Name == "Новое" && d.Article == "NEW"));
    }

    /// <summary>Ошибка API ловится и записывается в ErrorMessage.</summary>
    [Fact]
    public async Task Save_On_Error_Sets_ErrorMessage()
    {
        _productsClient.CreateAsync(Arg.Any<ProductRequestDto>())
            .Returns(Task.FromException<ProductDto>(new Exception("api boom")));

        var vm = new ProductFormViewModel(_popupService, _productsClient, _lookupsClient)
        {
            Name = "X",
            Article = "X",
            SelectedCategory = new CategoryDto { Id = 1 },
            SelectedUnit = new UnitOfMeasureDto { Id = 1 }
        };

        await vm.SaveCommand.ExecuteAsync(null);

        vm.HasError.Should().BeTrue();
        vm.ErrorMessage.Should().Contain("api boom");
    }

    /// <summary>AddNewCategoryAsync открывает попап и в OnSave создаёт категорию.</summary>
    [Fact]
    public async Task AddNewCategory_Creates_And_Selects_New_Category()
    {
        // Мокаем CreateCategoryAsync БЕЗ CT — его вызывает OnSave
        _lookupsClient.CreateCategoryAsync(Arg.Any<CategoryCreateDto>())
            .Returns(Task.FromResult(new CategoryDto { Id = 99, Name = "Новая" }));

        var vm = new ProductFormViewModel(_popupService, _productsClient, _lookupsClient);

        // Перехватываем вызов открытия попапа
        LookupPopupViewModel? captured = null;
        vm.ShowLookupPopupRequested = popupVm =>
        {
            captured = popupVm;
            return Task.CompletedTask;
        };

        await vm.AddNewCategoryCommand.ExecuteAsync(null);

        // Попап открыт
        captured.Should().NotBeNull();
        captured!.Title.Should().Contain("категории");

        // Имитируем ввод и нажатие «Сохранить»
        captured.Name = "Новая";
        await captured.SaveCommand.ExecuteAsync(null);

        // Категория добавлена (не null!) и выбрана
        vm.Categories.Should().ContainSingle(c => c.Id == 99);
        vm.SelectedCategory!.Id.Should().Be(99);
    }

    /// <summary>AddNewUnitAsync открывает попап и в OnSave создаёт единицу.</summary>
    [Fact]
    public async Task AddNewUnit_Creates_And_Selects_New_Unit()
    {
        _lookupsClient.CreateUnitAsync(Arg.Any<UnitOfMeasureCreateDto>())
            .Returns(Task.FromResult(new UnitOfMeasureDto { Id = 99, Name = "кг" }));

        var vm = new ProductFormViewModel(_popupService, _productsClient, _lookupsClient);

        LookupPopupViewModel? captured = null;
        vm.ShowLookupPopupRequested = v => { captured = v; return Task.CompletedTask; };

        await vm.AddNewUnitCommand.ExecuteAsync(null);

        captured.Should().NotBeNull();
        captured!.Name = "кг";
        await captured.SaveCommand.ExecuteAsync(null);

        vm.Units.Should().ContainSingle(u => u.Id == 99);
        vm.SelectedUnit!.Id.Should().Be(99);
    }
}
