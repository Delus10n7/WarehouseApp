namespace WarehouseApp.Maui.Controls;

public partial class BottomTabBar : ContentView
{
    public static readonly BindableProperty ActiveTabProperty =
        BindableProperty.Create(
            nameof(ActiveTab), typeof(string), typeof(BottomTabBar), string.Empty);

    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    public BottomTabBar()
    {
        InitializeComponent();
    }

    private async void OnTabTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not string route) return;
        if (route == ActiveTab) return; // уже на этой вкладке

        await Shell.Current.GoToAsync($"//{route}");
    }
}