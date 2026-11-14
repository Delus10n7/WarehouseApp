using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Extensions;
using WarehouseApp.Application.Dtos.WareDocumentDtos;
using WarehouseApp.Maui.ViewModels.Documents;
using WarehouseApp.Infrastructure.Client.Generated;

namespace WarehouseApp.Maui.Views.Documents;

/// <summary>
/// Страница для отображения списка документов и управления ими.
/// </summary>
public partial class DocumentsPage : ContentPage
{
    private readonly IPopupService _popupService;
    private readonly IDocumentsClient _documentsClient;
    private readonly IProductsClient _productsClient;
    private readonly ICounterpartiesClient _counterpartiesClient;
    private readonly DocumentsViewModel _viewModel;

    public DocumentsPage(
        IPopupService popupService,
        IDocumentsClient documentsClient,
        IProductsClient productsClient,
        ICounterpartiesClient counterpartiesClient,
        DocumentsViewModel viewModel)
    {
        InitializeComponent();
        _popupService = popupService;
        _documentsClient = documentsClient;
        _productsClient = productsClient;
        _counterpartiesClient = counterpartiesClient;
        _viewModel = viewModel;

        _viewModel.OpenDocumentFormRequested += OnOpenDocumentFormRequested;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await _viewModel.LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"OnAppearing failed: {ex}");
        }
    }

    private async void OnAddDocumentClicked(object? sender, EventArgs e) => await OpenDocumentFormAsync(null);

    private async Task OnOpenDocumentFormRequested(WareDocumentDto? document) => await OpenDocumentFormAsync(document);

    private async Task OpenDocumentFormAsync(WareDocumentDto? document)
    {
        var formVm = document is null
            ? new DocumentFormViewModel(_popupService, _documentsClient, _productsClient, _counterpartiesClient)
            : new DocumentFormViewModel(_popupService, _documentsClient, _productsClient, _counterpartiesClient, document);

        await formVm.InitializeAsync();

        var popup = new DocumentPopup(formVm);
        await this.ShowPopupAsync(popup, new PopupOptions { Shape = null });

        await _viewModel.LoadDataAsync();
    }
}
