using FluentAssertions;
using NSubstitute;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Maui.Services;
using WarehouseApp.Maui.ViewModels.Counterparties;

namespace WarehouseApp.Tests.ViewModels;

/// <summary>
/// Тесты для <see cref="CounterpartiesViewModel"/>. 
/// Проверяют логику загрузки данных, обработку ошибок и фильтрацию списка.
/// </summary>
public class CounterpartiesViewModelTests
{
    private readonly ICounterpartyApiService _apiServiceMock;

    /// <summary>
    /// Инициализирует тестовый класс, создавая заглушку (мок) для API-сервиса.
    /// </summary>
    public CounterpartiesViewModelTests()
    {
        _apiServiceMock = Substitute.For<ICounterpartyApiService>();
    }

    /// <summary>
    /// Проверяет, что при успешной загрузке данных коллекция заполняется контрагентами.
    /// </summary>
    [Fact]
    public async Task LoadDataAsync_Should_Populate_Collection_On_Success()
    {
        // Arrange (Подготовка)
        var fakeData = new List<Counterparty>
        {
            new() { Name = "ООО Ромашка" },
            new() { Name = "ИП Иванов" }
        };
        _apiServiceMock.GetAllAsync(Arg.Any<CancellationToken>()).Returns(fakeData);

        var vm = new CounterpartiesViewModel(_apiServiceMock);

        // Act (Действие)
        await vm.LoadDataAsync();

        // Assert (Проверка)
        vm.Counterparties.Should().HaveCount(2);
        vm.IsRefreshing.Should().BeFalse();
    }

    /// <summary>
    /// Проверяет, что при возникновении исключения в API, коллекция очищается, а индикатор загрузки отключается.
    /// </summary>
    [Fact]
    public async Task LoadDataAsync_Should_Clear_Collection_On_Exception()
    {
        // Arrange
        _apiServiceMock.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<Counterparty>>(new Exception("Ошибка сети")));

        var vm = new CounterpartiesViewModel(_apiServiceMock);

        // Act
        await vm.LoadDataAsync();

        // Assert
        vm.Counterparties.Should().BeEmpty();
        vm.IsRefreshing.Should().BeFalse();
    }

    /// <summary>
    /// Проверяет, что при вводе текста в поисковую строку список контрагентов корректно фильтруется.
    /// </summary>
    [Fact]
    public async Task OnSearchTextChanged_Should_Filter_Collection()
    {
        // Arrange
        var fakeData = new List<Counterparty>
        {
            new() { Name = "ООО Ромашка" },
            new() { Name = "ИП Иванов" }
        };
        _apiServiceMock.GetAllAsync(Arg.Any<CancellationToken>()).Returns(fakeData);

        var vm = new CounterpartiesViewModel(_apiServiceMock);
        await vm.LoadDataAsync(); // Загружаем данные в коллекцию

        // Act
        vm.SearchText = "Ромаш"; // Имитируем ввод в поле поиска

        // Assert
        vm.Counterparties.Should().HaveCount(1);
        vm.Counterparties.First().Name.Should().Be("ООО Ромашка");
    }
}
