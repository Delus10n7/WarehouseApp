using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using WarehouseApp.Application.Dtos.CounterpartyDtos;
using WarehouseApp.Infrastructure.Client.Generated;

namespace WarehouseApp.Maui.ViewModels.Counterparties;

/// <summary>
/// ViewModel страницы списка контрагентов.
/// </summary>
public partial class CounterpartiesViewModel : ObservableObject
{
    /// <summary>HTTP-клиент API контрагентов (сгенерирован NSwag).</summary>
    private readonly ICounterpartiesClient _counterpartiesClient;

    /// <summary>Логгер для записи ошибок загрузки и диагностики.</summary>
    private readonly ILogger<CounterpartiesViewModel> _logger;

    /// <summary>
    /// Полный «сырой» список контрагентов с сервера — хранится в памяти,
    /// чтобы фильтрация не делала повторных HTTP-запросов.
    /// </summary>
    private List<CounterpartyDto> _allCounterparties = new();

    /// <summary>
    /// Флаг «идёт обновление» — привязан к <c>RefreshView.IsRefreshing</c> в XAML.
    /// </summary>
    [ObservableProperty] private bool isRefreshing;

    /// <summary>
    /// Текст поиска — привязан к <c>Entry.Text</c> в XAML.
    /// Изменение вызывает <c>OnSearchTextChanged</c>, который перезапускает фильтрацию.
    /// </summary>
    [ObservableProperty] private string searchText = string.Empty;

    /// <summary>
    /// Отфильтрованный список контрагентов, привязанный к <c>CollectionView.ItemsSource</c>.
    /// </summary>
    public ObservableCollection<CounterpartyDto> Counterparties { get; } = new();

    /// <summary>
    /// Событие запроса редактирования — пользователь тапнул по карточке контрагента.
    /// </summary>
    public event Func<CounterpartyDto, Task>? EditRequested;

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="CounterpartiesViewModel"/>.
    /// </summary>
    /// <param name="counterpartiesClient">Сгенерированный клиент API контрагентов.</param>
    /// <param name="logger">Логгер для диагностики.</param>
    public CounterpartiesViewModel(
        ICounterpartiesClient counterpartiesClient,
        ILogger<CounterpartiesViewModel> logger)
    {
        // Сохраняем зависимости — оба должны быть non-null (DI гарантирует)
        _counterpartiesClient = counterpartiesClient;
        _logger = logger;
    }

    /// <summary>
    /// Асинхронно загружает список контрагентов с сервера и применяет фильтр.
    /// </summary>
    /// <returns>Задача, завершающаяся после окончания загрузки и обновления UI.</returns>
    [RelayCommand]
    public async Task LoadDataAsync()
    {
        // Включаем индикатор обновления; он будет выключен в finally
        IsRefreshing = true;

        try
        {
            // Запрашиваем данные. NSwag-клиент возвращает ICollection<T>,
            // поэтому приводим к List — фильтрация идёт через LINQ-to-Objects
            _allCounterparties = (await _counterpartiesClient.GetAllAsync()).ToList();

            // Применяем текущий фильтр (может быть пустым — тогда показываются все)
            ApplyFilter();
        }
        catch (Exception ex)
        {
            // Логируем ошибку, но не пробрасываем — UI покажет пустой список
            _logger.LogError(ex, "Ошибка загрузки контрагентов");

            // Обнуляем список, чтобы EmptyView показал заглушку
            _allCounterparties = new();
            ApplyFilter();
        }
        finally
        {
            // Гарантированно выключаем индикатор, даже если было исключение
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Срабатывает при изменении <see cref="SearchText"/> (генератор вызывает partial-метод).
    /// </summary>
    /// <param name="value">Новое значение текста поиска.</param>
    partial void OnSearchTextChanged(string value) => ApplyFilter();

    /// <summary>
    /// Фильтрует <see cref="_allCounterparties"/> по текущему <see cref="SearchText"/>
    /// и переносит результат в <see cref="Counterparties"/>.
    /// </summary>
    private void ApplyFilter()
    {
        // Полная очистка — самый простой способ гарантировать корректное обновление UI
        Counterparties.Clear();

        // Если поиск пустой — отдаём весь список, иначе — фильтруем по вхождению подстроки
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allCounterparties
            : _allCounterparties.Where(c =>
                // OrdinalIgnoreCase — быстрое регистронезависимое сравнение (не зависит от культуры)
                c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        // Копируем в ObservableCollection — CollectionView автоматически обновится
        foreach (var item in filtered)
            Counterparties.Add(item);
    }

    /// <summary>
    /// Команда редактирования: поднимает событие <see cref="EditRequested"/>.
    /// </summary>
    /// <param name="counterparty">Контрагент, по карточке которого тапнули.</param>
    /// <returns>Задача, завершающаяся после завершения всех обработчиков события.</returns>
    [RelayCommand]
    private async Task Edit(CounterpartyDto counterparty)
    {
        // Поднимаем событие только если есть подписчики (иначе пропускаем)
        if (EditRequested != null)
            await EditRequested.Invoke(counterparty);
    }
}
