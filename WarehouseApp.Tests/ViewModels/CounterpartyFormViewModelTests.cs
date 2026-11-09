using CommunityToolkit.Maui;
using FluentAssertions;
using NSubstitute;
using WarehouseApp.Application.Dtos.CounterpartyDtos;
using WarehouseApp.Infrastructure.Client.Generated;
using WarehouseApp.Maui.ViewModels.Counterparties;

namespace WarehouseApp.Tests.ViewModels;

/// <summary>
/// Unit-тесты <see cref="CounterpartyFormViewModel"/>.
/// </summary>
public class CounterpartyFormViewModelTests
{
    private readonly IPopupService _popupServiceMock;
    private readonly ICounterpartiesClient _clientMock;

    /// <summary>Инициализирует моки.</summary>
    public CounterpartyFormViewModelTests()
    {
        _popupServiceMock = Substitute.For<IPopupService>();
        _clientMock = Substitute.For<ICounterpartiesClient>();
    }

    /// <summary>Конструктор создания: IsEditMode = false, заголовок «Создать контрагента».</summary>
    [Fact]
    public void Ctor_Create_Sets_IsEditMode_False()
    {
        var vm = new CounterpartyFormViewModel(_popupServiceMock, _clientMock);

        vm.IsEditMode.Should().BeFalse();
        vm.Title.Should().Be("Создать контрагента");
        vm.ButtonText.Should().Be("Добавить контрагента");
    }

    /// <summary>Конструктор редактирования: IsEditMode = true и поля заполнены из DTO.</summary>
    [Fact]
    public void Ctor_Edit_Populates_Fields()
    {
        var existing = new CounterpartyDto
        {
            Id = 1,
            Name = "Ромашка",
            Phone = "+7",
            Email = "a@b.c"
        };

        var vm = new CounterpartyFormViewModel(_popupServiceMock, _clientMock, existing);

        vm.IsEditMode.Should().BeTrue();
        vm.Name.Should().Be("Ромашка");
        vm.Phone.Should().Be("+7");
        vm.Email.Should().Be("a@b.c");
    }

    /// <summary>Пустое Name — SetError, API не вызывается.</summary>
    [Fact]
    public async Task Save_With_Empty_Name_Sets_Error_And_Does_Not_Call_Api()
    {
        var vm = new CounterpartyFormViewModel(_popupServiceMock, _clientMock) { Name = "" };

        await vm.SaveCommand.ExecuteAsync(null);

        vm.HasError.Should().BeTrue();
        vm.ErrorMessage.Should().Contain("обязательно");
        await _clientMock.DidNotReceive()
            .CreateAsync(Arg.Any<CounterpartyRequestDto>());
    }

    /// <summary>Успешное создание: вызывается CreateAsync с правильным DTO.</summary>
    [Fact]
    public async Task Save_Create_Calls_CreateAsync()
    {
        // Мокаем перегрузку БЕЗ CancellationToken — её вызывает VM
        _clientMock.CreateAsync(Arg.Any<CounterpartyRequestDto>())
            .Returns(Task.FromResult(new CounterpartyDto { Id = 1 }));

        var vm = new CounterpartyFormViewModel(_popupServiceMock, _clientMock)
        {
            Name = "  Новый  ",
            Phone = "   ",
            Email = "  mail@test  "
        };

        await vm.SaveCommand.ExecuteAsync(null);

        await _clientMock.Received(1).CreateAsync(
            Arg.Is<CounterpartyRequestDto>(d =>
                d.Name == "Новый" &&
                d.Phone == null &&
                d.Email == "mail@test"));
    }

    /// <summary>Успешное редактирование: вызывается UpdateAsync с id и DTO.</summary>
    [Fact]
    public async Task Save_Edit_Calls_UpdateAsync()
    {
        // Мокаем перегрузку UpdateAsync(id, dto) без CancellationToken
        _clientMock.UpdateAsync(Arg.Any<int>(), Arg.Any<CounterpartyRequestDto>())
            .Returns(Task.CompletedTask);

        var existing = new CounterpartyDto { Id = 42, Name = "Старое" };
        var vm = new CounterpartyFormViewModel(_popupServiceMock, _clientMock, existing)
        {
            Name = "Новое"
        };

        await vm.SaveCommand.ExecuteAsync(null);

        await _clientMock.Received(1).UpdateAsync(
            42,
            Arg.Is<CounterpartyRequestDto>(d => d.Name == "Новое"));
    }

    /// <summary>Ошибка API ловится и записывается в ErrorMessage.</summary>
    [Fact]
    public async Task Save_On_Api_Error_Sets_ErrorMessage()
    {
        // Мокаем БЕСПАРАМЕТРИЧЕСКУЮ перегрузку
        _clientMock.CreateAsync(Arg.Any<CounterpartyRequestDto>())
            .Returns(Task.FromException<CounterpartyDto>(
                new InvalidOperationException("server down")));

        var vm = new CounterpartyFormViewModel(_popupServiceMock, _clientMock) { Name = "Тест" };

        await vm.SaveCommand.ExecuteAsync(null);

        vm.HasError.Should().BeTrue();
        vm.ErrorMessage.Should().Contain("server down");
    }
}
