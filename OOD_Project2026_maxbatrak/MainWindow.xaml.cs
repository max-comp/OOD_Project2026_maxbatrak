using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OOD_Project2026_maxbatrak.Data;
using OOD_Project2026_maxbatrak.Models;

namespace OOD_Project2026_maxbatrak
{
    public partial class MainWindow : Window
    {
        private TripPlannerContext db;
        private ObservableCollection<Trip> trips;

        public MainWindow()
        {
            InitializeComponent();
            db = new TripPlannerContext();
            LoadTrips();
        }

        // get selected trip
        private Trip SelectedTrip
        {
            get { return TripsListBox.SelectedItem as Trip; }
        }

        private void LoadTrips()
        {
            try
            {
                db.Database.CreateIfNotExists();

                var tripList = db.Trips
                    .Include("ItineraryItems")
                    .Include("Bookings")
                    .Include("Expenses")
                    .ToList();

                trips = new ObservableCollection<Trip>(tripList);
                TripsListBox.ItemsSource = trips;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not load trips from database.\n\n{ex.Message}",
                    "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshTripLists()
        {
            if (SelectedTrip == null) return;

            var trip = SelectedTrip;

            // Refresh the itinerary day list
            DaysListBox.ItemsSource = trip.GetTripDays()
                .Select(d => d.ToString("ddd, MMM dd")).ToList();

            // Refresh bookings and expenses lists
            BookingsListView.ItemsSource = new ObservableCollection<Booking>(trip.Bookings);
            ExpensesListView.ItemsSource = new ObservableCollection<Expense>(trip.Expenses);

            // Refresh summary
            UpdateSummary(trip);
        }

        private void UpdateSummary(Trip trip)
        {
            SummaryPlaceholder.Visibility = Visibility.Collapsed;
            SummaryPanel.Visibility = Visibility.Visible;

            SummaryName.Text = trip.Name;
            SummaryDates.Text = $"{trip.StartDate:ddd, MMM dd} – {trip.EndDate:ddd, MMM dd}";
            SummaryItineraryCount.Text = $"{trip.ItineraryItems.Count} item(s)";
            SummaryBookingsCount.Text = $"{trip.Bookings.Count} booking(s)";
            SummaryTotal.Text = $"Total: {trip.TotalExpenses:C}";
            SummaryPaid.Text = $"Paid: {trip.TotalPaid:C}";
            SummaryOwed.Text = $"Owed: {trip.TotalOwed:C}";
        }

        //HOME TAB
        private void TripsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedTrip == null)
            {
                SummaryPlaceholder.Visibility = Visibility.Visible;
                SummaryPanel.Visibility = Visibility.Collapsed;
                return;
            }

            RefreshTripLists();
        }

        private void AddTrip_Click(object sender, RoutedEventArgs e)
        {
            // Simple input
            var dialog = new Window
            {
                Title = "New Trip",
                Width = 320,
                Height = 280,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };

            var stack = new StackPanel { Margin = new Thickness(16) };

            stack.Children.Add(new TextBlock { Text = "Trip Name" });
            var nameBox = new TextBox { Margin = new Thickness(0, 4, 0, 8) };
            stack.Children.Add(nameBox);

            stack.Children.Add(new TextBlock { Text = "Start Date" });
            var startPicker = new DatePicker { Margin = new Thickness(0, 4, 0, 8) };
            stack.Children.Add(startPicker);

            stack.Children.Add(new TextBlock { Text = "End Date" });
            var endPicker = new DatePicker { Margin = new Thickness(0, 4, 0, 12) };
            stack.Children.Add(endPicker);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okBtn = new Button { Content = "Create", Margin = new Thickness(0, 0, 8, 0), Padding = new Thickness(16, 4, 16, 4) };
            var cancelBtn = new Button { Content = "Cancel", Padding = new Thickness(16, 4, 16, 4) };

            okBtn.Click += (s, args) => { dialog.DialogResult = true; };
            cancelBtn.Click += (s, args) => { dialog.DialogResult = false; };

            btnPanel.Children.Add(okBtn);
            btnPanel.Children.Add(cancelBtn);
            stack.Children.Add(btnPanel);

            dialog.Content = stack;

            if (dialog.ShowDialog() == true)
            {
                if (string.IsNullOrWhiteSpace(nameBox.Text) || !startPicker.SelectedDate.HasValue || !endPicker.SelectedDate.HasValue)
                {
                    MessageBox.Show("Please fill in all fields.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var trip = new Trip(nameBox.Text.Trim(), startPicker.SelectedDate.Value, endPicker.SelectedDate.Value);
                db.Trips.Add(trip);
                db.SaveChanges();

                trips.Add(trip);
                TripsListBox.SelectedItem = trip;
            }
        }

        //Itenerary Tab
        private void DaysListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedTrip == null || DaysListBox.SelectedIndex < 0) return;

            var days = SelectedTrip.GetTripDays();
            var selectedDay = days[DaysListBox.SelectedIndex];

            ItineraryListView.ItemsSource = new ObservableCollection<ItineraryItem>(
                SelectedTrip.GetItemsForDay(selectedDay));
        }

        private void ItineraryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ItineraryListView.SelectedItem as ItineraryItem;
            if (item == null) return;

            // Load selected item into the form for viewing
            ItineraryTitleBox.Text = item.Title;
            ItineraryTypeCombo.SelectedIndex = (int)item.ItemType;
            ItineraryDatePicker.SelectedDate = item.Date;
            ItineraryTimeBox.Text = item.Time.ToString(@"hh\:mm");
        }

        private void AddItineraryItem_Click(object sender, RoutedEventArgs e)
        {
            ClearItineraryForm();
        }

        private void SaveItineraryItem_Click(object sender, RoutedEventArgs e)
        {
            // TODO: validate fields, create/update ItineraryItem, save to database
            MessageBox.Show("Save not yet implemented – coming soon.", "TODO", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelItineraryItem_Click(object sender, RoutedEventArgs e)
        {
            ClearItineraryForm();
        }

        private void ClearItineraryForm()
        {
            ItineraryTitleBox.Text = "";
            ItineraryTypeCombo.SelectedIndex = -1;
            ItineraryDatePicker.SelectedDate = null;
            ItineraryTimeBox.Text = "";
        }

        //BOOKINGS TAB
        private void BookingsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var booking = BookingsListView.SelectedItem as Booking;
            if (booking == null) return;

            // Load selected booking into the form for viewing
            BookingReferenceBox.Text = booking.Reference;
            BookingTypeCombo.SelectedIndex = (int)booking.BookingType;
            BookingDatePicker.SelectedDate = booking.CheckInDate;
            BookingFromBox.Text = booking.From;
            BookingToBox.Text = booking.To;
        }

        private void AddBooking_Click(object sender, RoutedEventArgs e)
        {
            ClearBookingForm();
        }

        private void SaveBooking_Click(object sender, RoutedEventArgs e)
        {
            // TODO: validate fields, create/update Booking, save to database
            MessageBox.Show("Save not yet implemented – coming soon.", "TODO", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelBooking_Click(object sender, RoutedEventArgs e)
        {
            ClearBookingForm();
        }

        private void ClearBookingForm()
        {
            BookingReferenceBox.Text = "";
            BookingTypeCombo.SelectedIndex = -1;
            BookingDatePicker.SelectedDate = null;
            BookingFromBox.Text = "";
            BookingToBox.Text = "";
        }

        //BUDGETING TAB
        private void ExpensesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var expense = ExpensesListView.SelectedItem as Expense;
            if (expense == null) return;

            // Load selected expense into the form for viewing
            ExpenseCategoryCombo.SelectedIndex = (int)expense.Category;
            ExpenseTitleBox.Text = expense.Title;
            ExpenseAmountBox.Text = expense.Amount.ToString("F2");
            ExpensePaidCombo.SelectedIndex = expense.IsPaid ? 0 : 1;
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            ClearExpenseForm();
        }

        private void SaveExpense_Click(object sender, RoutedEventArgs e)
        {
            // TODO: validate fields, create/update Expense, save to database
            MessageBox.Show("Save not yet implemented – coming soon.", "TODO", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelExpense_Click(object sender, RoutedEventArgs e)
        {
            ClearExpenseForm();
        }

        private void ClearExpenseForm()
        {
            ExpenseCategoryCombo.SelectedIndex = -1;
            ExpenseTitleBox.Text = "";
            ExpenseAmountBox.Text = "";
            ExpensePaidCombo.SelectedIndex = -1;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            db?.Dispose();
        }
    }
}
