using CommunityToolkit.Maui;
using FluentAssertions;
using NSubstitute;
using WarehouseApp.Application.Dtos.CommonDtos;
using WarehouseApp.Application.Dtos.CounterpartyDtos;
using WarehouseApp.Application.Dtos.ProductDtos;
using WarehouseApp.Application.Dtos.WareDocumentDtos;
using WarehouseApp.Domain.Entities.Enums;
using WarehouseApp.Infrastructure.Client.Generated;
using WarehouseApp.Maui.ViewModels.Documents;

namespace WarehouseApp.Tests.ViewModels;

/// <summary>
/// Unit-тесты <see cref="DocumentFormViewModel"/>.
/// </summary>
public class DocumentFormViewModelTests
{
    private readonly IPopupService _popupService;
    private readonly IDocumentsClient _documentsClient;
    private readonly IProductsClient _productsClient;
    private readonly ICounterpartiesClient _counterpartiesClient;

    public DocumentFormViewModelTests()
    {
        _popupService = Substitute.For<IPopupService>();
        _documentsClient = Substitute.For<IDocumentsClient>();
        _productsClient = Substitute.For<IProductsClient>();
        _counterpartiesClient = Substitute.For<ICounterpartiesClient>();
    }

    /// <summary>
    /// Проверяет, что в режиме создания заголовок соответствует выбранному типу.
    /// </summary>
    [Fact]
    public void Ctor_Create_Sets_Default_State()
    {
        var vm = new DocumentFormViewModel(_popupService, _documentsClient, _productsClient, _counterpartiesClient);

        vm.IsEditMode.Should().BeFalse();
        vm.IsReceipt.Should().BeTrue();
        vm.Title.Should().Be("Создание документа");
        vm.CounterpartyLabel.Should().Be("Поставщик");
    }

    /// <summary>
    /// Проверяет, что при переключении типа меняется заголовок и метка контрагента.
    /// </summary>
    [Fact]
    public void ToggleType_Changes_Labels()
    {
        var vm = new DocumentFormViewModel(_popupService, _documentsClient, _productsClient, _counterpartiesClient);

        vm.ToggleTypeCommand.Execute("Sale");

        vm.IsReceipt.Should().BeFalse();
        vm.CounterpartyLabel.Should().Be("Покупатель");
        vm.Title.Should().Be("Создание документа Продажа");
    }

    /// <summary>
    /// Проверяет, что QuantityLabel обновляется при выборе товара.
    /// </summary>
    [Fact]
    public void OnSelectedProductChanged_Updates_QuantityLabel()
    {
        var vm = new DocumentFormViewModel(_popupService, _documentsClient, _productsClient, _counterpartiesClient);

        vm.SelectedProduct = new ProductDto { UnitOfMeasureName = "мл" };

        vm.QuantityLabel.Should().Be("Количество, мл");
    }

    /// <summary>
    /// Проверяет, что InitializeAsync загружает справочники и восстанавливает данные при редактировании.
    /// </summary>
    [Fact]
    public async Task InitializeAsync_Loads_And_Restores_Data()
    {
        _productsClient.GetAllAsync().Returns(Task.FromResult<ICollection<ProductDto>>(new List<ProductDto>
        {
            new() { Id = 5, Name = "Товар", UnitOfMeasureName = "шт" }
        }));
        _counterpartiesClient.GetAllAsync().Returns(Task.FromResult<ICollection<CounterpartyDto>>(new List<CounterpartyDto>
        {
            new() { Id = 7, Name = "Контрагент" }
        }));

        var existing = new WareDocumentDto
        {
            Id = 1,
            Type = WareDocumentType.Receipt,
            CounterpartyId = 7,
            Items = new List<WareDocumentItemDto> { new() { ProductId = 5, Quantity = 10, Price = 100 } }
        };

        var vm = new DocumentFormViewModel(_popupService, _documentsClient, _productsClient, _counterpartiesClient, existing);
        await vm.InitializeAsync();

        vm.Products.Should().HaveCount(1);
        vm.Counterparties.Should().HaveCount(1);
        vm.SelectedProduct!.Id.Should().Be(5);
        vm.SelectedCounterparty!.Id.Should().Be(7);
        vm.QuantityText.Should().Be("10");
        vm.AmountText.Should().Be("1000");
    }

    /// <summary>
    /// Проверяет, что SaveAsync вызывает CreateAsync с корректным DTO.
    /// </summary>
    [Fact]
    public async Task SaveAsync_Create_Calls_CreateAsync_With_Correct_Dto()
    {
        _documentsClient.CreateAsync(Arg.Any<WareDocumentCreateDto>())
            .Returns(Task.FromResult(new WareDocumentDto()));

        var vm = new DocumentFormViewModel(_popupService, _documentsClient, _productsClient, _counterpartiesClient)
        {
            IsReceipt = false,
            SelectedProduct = new ProductDto { Id = 5 },
            SelectedCounterparty = new CounterpartyDto { Id = 7 },
            QuantityText = "10",
            AmountText = "500" // цена за ед. = 50
        };

        await vm.SaveCommand.ExecuteAsync(null);

        await _documentsClient.Received(1).CreateAsync(Arg.Is<WareDocumentCreateDto>(dto =>
            dto.Type == WareDocumentType.Sale &&
            dto.CounterpartyId == 7 &&
            dto.Items.Count == 1 &&
            dto.Items[0].ProductId == 5 &&
            dto.Items[0].Quantity == 10 &&
            dto.Items[0].Price == 50
        ));
    }

    /// <summary>
    /// Проверяет, что ошибка ApiException с ProblemDetails корректно парсится в ErrorMessage.
    /// </summary>
    [Fact]
    public async Task SaveAsync_Catches_ProblemDetails_And_Sets_ErrorMessage()
    {
        var problem = new ProblemDetails { Title = "Ошибка", Detail = "Недостаточно товара на складе" };
        var apiException = new ApiException<ProblemDetails>("error", 400, "{}", new Dictionary<string, IEnumerable<string>>(), problem, null!);

        _documentsClient.CreateAsync(Arg.Any<WareDocumentCreateDto>())
            .Returns(Task.FromException<WareDocumentDto>(apiException));

        var vm = new DocumentFormViewModel(_popupService, _documentsClient, _productsClient, _counterpartiesClient)
        {
            SelectedProduct = new ProductDto { Id = 1 },
            SelectedCounterparty = new CounterpartyDto { Id = 1 },
            QuantityText = "10",
            AmountText = "1000"
        };

        await vm.SaveCommand.ExecuteAsync(null);

        vm.HasError.Should().BeTrue();
        vm.ErrorMessage.Should().Be("Недостаточно товара на складе");
    }

    /// <summary>
    /// Проверяет, что валидация не пропускает пустые поля.
    /// </summary>
    [Fact]
    public async Task SaveAsync_Validation_Stops_Execution_On_Empty_Fields()
    {
        var vm = new DocumentFormViewModel(_popupService, _documentsClient, _productsClient, _counterpartiesClient);
        // все поля пустые

        await vm.SaveCommand.ExecuteAsync(null);

        vm.HasError.Should().BeTrue();
        vm.ErrorMessage.Should().Contain("Заполните все поля");
        await _documentsClient.DidNotReceive().CreateAsync(Arg.Any<WareDocumentCreateDto>());
    }
}
