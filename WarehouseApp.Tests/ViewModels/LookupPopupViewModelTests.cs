using FluentAssertions;
using WarehouseApp.Maui.ViewModels.Products;

namespace WarehouseApp.Tests.ViewModels;

/// <summary>
/// Unit-тесты <see cref="LookupPopupViewModel"/>.
/// </summary>
public class LookupPopupViewModelTests
{
    /// <summary>
    /// Конструктор копирует параметры в свойства.
    /// </summary>
    [Fact]
    public void Ctor_Copies_Parameters_To_Properties()
    {
        var vm = new LookupPopupViewModel("Заголовок", "Плейсхолдер", "Кнопка");

        vm.Title.Should().Be("Заголовок");
        vm.Placeholder.Should().Be("Плейсхолдер");
        vm.ButtonText.Should().Be("Кнопка");
    }

    /// <summary>SaveAsync при пустом имени не вызывает ни OnSave, ни CloseAction.</summary>
    [Fact]
    public async Task Save_With_Empty_Name_Does_Nothing()
    {
        var vm = new LookupPopupViewModel("t", "p", "b") { Name = "   " };

        var onSaveCalled = false;
        var closeCalled = false;
        vm.OnSave = _ => { onSaveCalled = true; return Task.CompletedTask; };
        vm.CloseAction = () => { closeCalled = true; return Task.CompletedTask; };

        await vm.SaveCommand.ExecuteAsync(null);

        onSaveCalled.Should().BeFalse();
        closeCalled.Should().BeFalse();
    }

    /// <summary>SaveAsync с валидным именем вызывает OnSave и CloseAction, имя обрезается.</summary>
    [Fact]
    public async Task Save_With_Valid_Name_Calls_OnSave_And_CloseAction()
    {
        var vm = new LookupPopupViewModel("t", "p", "b") { Name = "  Категория  " };

        string? savedName = null;
        var closeCalled = false;
        vm.OnSave = name => { savedName = name; return Task.CompletedTask; };
        vm.CloseAction = () => { closeCalled = true; return Task.CompletedTask; };

        await vm.SaveCommand.ExecuteAsync(null);

        savedName.Should().Be("Категория");      // Trim
        closeCalled.Should().BeTrue();
    }
}
