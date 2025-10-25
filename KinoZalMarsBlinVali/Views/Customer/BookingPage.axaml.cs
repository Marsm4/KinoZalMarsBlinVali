using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using KinoZalMarsBlinVali.Data;
using KinoZalMarsBlinVali.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace KinoZalMarsBlinVali.Views
{
    public partial class BookingPage : UserControl
    {
        private Session _session;
        private List<HallSeat> _availableSeats = new List<HallSeat>();
        private ObservableCollection<SeatSelection> _selectedSeats = new ObservableCollection<SeatSelection>();
        private List<TicketType> _ticketTypes = new List<TicketType>();

        public class SeatRow
        {
            public int RowNumber { get; set; }
            public ObservableCollection<SeatInfo> Seats { get; set; } = new ObservableCollection<SeatInfo>();
        }

        public class SeatInfo
        {
            public HallSeat Seat { get; set; }
            public string SeatInfoText => $"Ряд {Seat.RowNumber}, Место {Seat.SeatNumber}"; // Переименовано
            public bool IsAvailable { get; set; }
            public bool IsSelected { get; set; }
        }

        public class SeatSelection
        {
            public HallSeat Seat { get; set; }
            public string SeatInfoText => $"Ряд {Seat.RowNumber}, Место {Seat.SeatNumber}"; // Переименовано
            public decimal Price { get; set; }
        }

        public BookingPage(Session session)
        {
            _session = session;
            InitializeComponent();
            LoadSessionInfo();
            LoadSeats();
            LoadTicketTypes();
        }

        private void LoadSessionInfo()
        {
            TitleText.Text = $"Бронирование: {_session.Movie.Title}";
            MovieTitle.Text = _session.Movie.Title;
            SessionInfo.Text = $"{_session.StartTime:dd.MM.yyyy HH:mm} - {_session.EndTime:HH:mm}";
            HallInfo.Text = $"Зал: {_session.Hall.HallName}";

            // Используем PosterPath вместо PosterUrl
            if (!string.IsNullOrEmpty(_session.Movie.PosterPath))
            {
                // Здесь должна быть логика загрузки изображения
            }
        }

        private void LoadSeats()
        {
            try
            {
                // Получаем все места в зале
                var hallSeats = AppDataContext.DbContext.HallSeats
                    .Where(s => s.HallId == _session.HallId && s.IsActive == true)
                    .OrderBy(s => s.RowNumber)
                    .ThenBy(s => s.SeatNumber)
                    .ToList();

                // Получаем занятые места на этот сеанс
                var occupiedSeatIds = AppDataContext.DbContext.Tickets
                    .Where(t => t.SessionId == _session.SessionId &&
                               (t.Status == "sold" || t.Status == "reserved"))
                    .Select(t => t.SeatId)
                    .ToList();

                // Создаем структуру рядов и мест
                var seatRows = new ObservableCollection<SeatRow>();
                var rows = hallSeats.GroupBy(s => s.RowNumber).OrderBy(g => g.Key);

                foreach (var rowGroup in rows)
                {
                    var seatRow = new SeatRow { RowNumber = rowGroup.Key };

                    foreach (var seat in rowGroup.OrderBy(s => s.SeatNumber))
                    {
                        var seatInfo = new SeatInfo
                        {
                            Seat = seat,
                            IsAvailable = !occupiedSeatIds.Contains(seat.SeatId) && seat.SeatType != "disabled"
                        };
                        seatRow.Seats.Add(seatInfo);
                    }

                    seatRows.Add(seatRow);
                }

                SeatsItemsControl.ItemsSource = seatRows;

                // Обновляем классы кнопок после загрузки данных
                UpdateSeatButtonsClasses();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки мест: {ex.Message}");
            }
        }
        private void UpdateSeatButtonsClasses()
        {
            // Ждем немного чтобы ItemsControl успел создать визуальные элементы
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var seatRow in SeatsItemsControl.Items)
                {
                    if (seatRow is SeatRow row)
                    {
                        foreach (var seatInfo in row.Seats)
                        {
                            // Находим кнопку для этого места
                            var container = SeatsItemsControl.ContainerFromItem(seatRow);
                            if (container != null)
                            {
                                // Здесь нужно найти конкретную кнопку по данным
                                // Это сложно сделать напрямую, поэтому проще обновлять классы в SeatButton_Click
                            }
                        }
                    }
                }
            }, Avalonia.Threading.DispatcherPriority.Background);
        }

        private void LoadTicketTypes()
        {
            try
            {
                _ticketTypes = AppDataContext.DbContext.TicketTypes.ToList();
                TicketTypeComboBox.ItemsSource = _ticketTypes;
                if (_ticketTypes.Any())
                    TicketTypeComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки типов билетов: {ex.Message}");
            }
        }

        private void SeatButton_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is SeatInfo seatInfo && seatInfo.IsAvailable)
            {
                // Сначала обновляем классы типа места
                UpdateSeatTypeClass(button, seatInfo.Seat.SeatType);

                if (seatInfo.IsSelected)
                {
                    // Убираем из выбранных
                    seatInfo.IsSelected = false;
                    button.Classes.Remove("selected");
                    var selected = _selectedSeats.FirstOrDefault(s => s.Seat.SeatId == seatInfo.Seat.SeatId);
                    if (selected != null)
                        _selectedSeats.Remove(selected);
                }
                else
                {
                    // Добавляем в выбранные
                    seatInfo.IsSelected = true;
                    button.Classes.Add("selected");
                    var basePrice = _session.BasePrice;
                    var seatMultiplier = seatInfo.Seat.PriceMultiplier ?? 1.0m;
                    var ticketType = TicketTypeComboBox.SelectedItem as TicketType;
                    var discount = ticketType?.DiscountPercent ?? 0;

                    var finalPrice = basePrice * seatMultiplier * (1 - discount / 100);

                    _selectedSeats.Add(new SeatSelection
                    {
                        Seat = seatInfo.Seat,
                        Price = finalPrice
                    });
                }

                UpdateTotalPrice();
                SelectedSeatsItemsControl.ItemsSource = _selectedSeats;
            }
        }
        private void UpdateSeatTypeClass(Button button, string seatType)
        {
            // Очищаем классы типа места
            button.Classes.Remove("vip");
            button.Classes.Remove("disabled");
            button.Classes.Remove("standard");

            // Добавляем нужный класс
            if (!string.IsNullOrEmpty(seatType))
            {
                button.Classes.Add(seatType);
            }
        }

        private void TicketType_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            // Пересчитываем цены при смене типа билета
            var ticketType = TicketTypeComboBox.SelectedItem as TicketType;
            if (ticketType != null)
            {
                foreach (var selectedSeat in _selectedSeats)
                {
                    var basePrice = _session.BasePrice;
                    var seatMultiplier = selectedSeat.Seat.PriceMultiplier ?? 1.0m; // Исправлено: добавлено ?? 1.0m
                    var discount = ticketType.DiscountPercent ?? 0; // Исправлено: добавлено ?? 0
                    selectedSeat.Price = basePrice * seatMultiplier * (1 - discount / 100);
                }
                UpdateTotalPrice();
                SelectedSeatsItemsControl.ItemsSource = _selectedSeats;
            }
        }

        private void UpdateTotalPrice()
        {
            var total = _selectedSeats.Sum(s => s.Price);
            TotalPriceText.Text = $"{total:0}₽";
        }

        private async void ConfirmBooking_Click(object? sender, RoutedEventArgs e)
        {
            if (!_selectedSeats.Any())
            {
                await ShowError("Выберите хотя бы одно место");
                return;
            }

            try
            {
                var customerId = AppDataContext.CurrentUser?.EmployeeId; // CustomerId хранится в EmployeeId для зрителя
                var ticketType = TicketTypeComboBox.SelectedItem as TicketType;

                foreach (var selectedSeat in _selectedSeats)
                {
                    var ticket = new Ticket
                    {
                        SessionId = _session.SessionId,
                        SeatId = selectedSeat.Seat.SeatId,
                        CustomerId = customerId,
                        TicketTypeId = ticketType?.TypeId ?? 1,
                        FinalPrice = selectedSeat.Price,
                        Status = "reserved",
                        ReservationTime = DateTime.Now,
                        ReservationExpires = DateTime.Now.AddMinutes(30), // Бронь на 30 минут
                        PurchaseTime = DateTime.Now
                    };

                    AppDataContext.DbContext.Tickets.Add(ticket);
                }

                AppDataContext.DbContext.SaveChanges();

                var successDialog = new MessageWindow("Успех",
                    $"Билеты успешно забронированы! У вас есть 30 минут для оплаты.");
                await successDialog.ShowDialog((Window)this.VisualRoot);

                // Возвращаемся к списку сеансов
                Back_Click(sender, e);
            }
            catch (Exception ex)
            {
                await ShowError($"Ошибка бронирования: {ex.Message}");
            }
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            if (this.Parent is ContentControl contentControl &&
                contentControl.Parent is CustomerMainPage mainPage)
            {
                mainPage.MainContentControl.Content = new CustomerSessionsPage();
            }
        }

        private async Task ShowError(string message)
        {
            var dialog = new MessageWindow("Ошибка", message);
            await dialog.ShowDialog((Window)this.VisualRoot);
        }
    }
}