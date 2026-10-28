using FluentAssertions;
using WarehouseApp.Maui.ViewModels.Products;

namespace WarehouseApp.Tests.ViewModels;

/// <summary>
/// Тесты для <see cref="LookupPopupViewModel"/>. 
/// Проверяют логику сохранения и закрытия.
/// </summary>
public class LookupPopupViewModelTests
{
    [Fact]
    public async Task SaveAsync_Should_Call_OnSave_And_CloseAction_When_Name_Is_Valid()
    {
        // Arrange
        var vm = new LookupPopupViewModel("Заголовок", "Плейсхолдер", "Кнопка");
        vm.Name = "Новая сущность";

        bool saveCalled = false;
        bool closeCalled = false;

        vm.OnSave = (name) => { saveCalled = true; return Task.CompletedTask; };
        vm.CloseAction = () => { closeCalled = true; return Task.CompletedTask; };

        // Act
        await vm.SaveCommand.ExecuteAsync(null);

        // Assert
        saveCalled.Should().BeTrue();
        closeCalled.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_Should_Not_Save_When_Name_Is_Empty()
    {
        // Arrange
        var vm = new LookupPopupViewModel("Заголовок", "Плейсхолдер", "Кнопка");
        vm.Name = "";

        bool saveCalled = false;
        vm.OnSave = (name) => { saveCalled = true; return Task.CompletedTask; };

        // Act
        await vm.SaveCommand.ExecuteAsync(null);

        // Assert
        saveCalled.Should().BeFalse();
    }
}
