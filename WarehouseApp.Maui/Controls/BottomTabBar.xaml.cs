namespace WarehouseApp.Maui.Controls;
using CommunityToolkit.Mvvm.Input;

public partial class BottomTabBar : ContentView
{
    public static readonly BindableProperty ActiveTabProperty =
        BindableProperty.Create(nameof(ActiveTab), typeof(string), typeof(BottomTabBar), string.Empty);

    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    public IAsyncRelayCommand<string> OnTabTappedCommand { get; }

    public BottomTabBar()
    {
        OnTabTappedCommand = new AsyncRelayCommand<string>(OnTabTappedAsync);
        InitializeComponent();
    }

    private async Task OnTabTappedAsync(string? route)
    {
        if (string.IsNullOrEmpty(route) || route == ActiveTab) return;
        await Shell.Current.GoToAsync($"//{route}");
    }
}