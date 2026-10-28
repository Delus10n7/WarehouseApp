using CommunityToolkit.Maui;
using FluentAssertions;
using NSubstitute;
using WarehouseApp.Domain.Entities;
using WarehouseApp.Application.Services;
using WarehouseApp.Maui.ViewModels.Counterparties;

namespace WarehouseApp.Tests.ViewModels;

/// <summary>
/// Тесты для <see cref="CounterpartyFormViewModel"/>. 
/// Проверяют валидацию полей, логику создания и обновления контрагента.
/// </summary>
public class CounterpartyFormViewModelTests
{
    private readonly IPopupService _popupServiceMock;
    private readonly ICounterpartyApiService _apiServiceMock;

    /// <summary>
    /// Инициализирует тестовый класс, создавая заглушки для сервисов попапов и API.
    /// </summary>
    public CounterpartyFormViewModelTests()
    {
        _popupServiceMock = Substitute.For<IPopupService>();
        _apiServiceMock = Substitute.For<ICounterpartyApiService>();
    }

    /// <summary>
    /// Проверяет, что при пустом имени устанавливается флаг ошибки и API не вызывается.
    /// </summary>
    [Fact]
    public async Task SaveAsync_Should_Set_Error_When_Name_Is_Empty()
    {
        // Arrange
        var vm = new CounterpartyFormViewModel(_popupServiceMock, _apiServiceMock);
        vm.Name = ""; // Оставляем имя пустым

        // Act
        await vm.SaveCommand.ExecuteAsync(null);

        // Assert
        vm.HasError.Should().BeTrue();
        vm.ErrorMessage.Should().Contain("обязательно");

        // Проверяем, что запрос к API НЕ отправлялся
        await _apiServiceMock.DidNotReceive().AddAsync(Arg.Any<Counterparty>());
    }

    /// <summary>
    /// Проверяет, что при создании нового контрагента вызывается метод AddAsync API-сервиса.
    /// </summary>
    [Fact]
    public async Task SaveAsync_Should_Call_AddAsync_On_Create()
    {
        // Arrange
        var vm = new CounterpartyFormViewModel(_popupServiceMock, _apiServiceMock);
        vm.Name = "Новый Контрагент";
        vm.Phone = "+79990000000";

        // Act
        await vm.SaveCommand.ExecuteAsync(null);

        // Assert
        vm.HasError.Should().BeFalse();
        // Проверяем, что API был вызван ровно 1 раз с правильным именем
        await _apiServiceMock.Received(1).AddAsync(
            Arg.Is<Counterparty>(c => c.Name == "Новый Контрагент"),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Проверяет, что при редактировании существующего контрагента вызывается метод UpdateAsync API-сервиса.
    /// </summary>
    [Fact]
    public async Task SaveAsync_Should_Call_UpdateAsync_On_Edit()
    {
        // Arrange
        var existing = new Counterparty { Id = 1, Name = "Старое Имя" };
        var vm = new CounterpartyFormViewModel(_popupServiceMock, _apiServiceMock, existing);

        vm.Name = "Новое Имя";

        // Act
        await vm.SaveCommand.ExecuteAsync(null);

        // Assert
        await _apiServiceMock.Received(1).UpdateAsync(
            Arg.Is<Counterparty>(c => c.Id == 1 && c.Name == "Новое Имя"),
            Arg.Any<CancellationToken>());
    }
}
