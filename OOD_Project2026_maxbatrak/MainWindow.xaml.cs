using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OOD_Project2026_maxbatrak.Data;
using OOD_Project2026_maxbatrak.Models;
using OOD_Project2026_maxbatrak.Services;

namespace OOD_Project2026_maxbatrak
{
    public partial class MainWindow : Window
    {
        private TripPlannerContext db;
        private ObservableCollection<Trip> trips;

        // Track which item is being edited (null = adding new)
        private ItineraryItem editingItineraryItem;
        private Booking editingBooking;
        private Expense editingExpense;

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
                WelcomePanel.Visibility = Visibility.Visible;
                CountryPanel.Visibility = Visibility.Collapsed;
                return;
            }

            RefreshTripLists();
            ShowCountryInfo(SelectedTrip);
        }

        private void ShowCountryInfo(Trip trip)
        {
            if (!string.IsNullOrEmpty(trip.FlagUrl))
            {
                try
                {
                    FlagImage.Source = new BitmapImage(new Uri(trip.FlagUrl));
                }
                catch
                {
                    FlagImage.Source = null;
                }

                CountryNameText.Text     = trip.Destination;
                CountryCapitalText.Text  = string.Empty;
                CountryCurrencyText.Text = !string.IsNullOrEmpty(trip.CurrencyCode)
                    ? $"Currency: {trip.CurrencyCode}"
                    : string.Empty;
                CountryRegionText.Text   = string.Empty;

                // Fetch fresh details (capital, region) from the API
                CountryInfo info = CountryService.GetCountryInfo(trip.Destination);
                if (info != null)
                {
                    CountryCapitalText.Text = !string.IsNullOrEmpty(info.Capital) ? $"Capital: {info.Capital}" : string.Empty;
                    CountryRegionText.Text  = !string.IsNullOrEmpty(info.Region)  ? $"Region: {info.Region}"   : string.Empty;
                    if (!string.IsNullOrEmpty(info.CurrencyName))
                        CountryCurrencyText.Text = $"Currency: {info.CurrencyCode} – {info.CurrencyName}";
                }

                WelcomePanel.Visibility = Visibility.Collapsed;
                CountryPanel.Visibility = Visibility.Visible;
            }
            else
            {
                WelcomePanel.Visibility = Visibility.Visible;
                CountryPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void AddTrip_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Window
            {
                Title = "New Trip",
                Width = 320,
                Height = 320,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };

            var stack = new StackPanel { Margin = new Thickness(16) };

            stack.Children.Add(new TextBlock { Text = "Trip Name" });
            var nameBox = new TextBox { Margin = new Thickness(0, 4, 0, 8) };
            stack.Children.Add(nameBox);

            stack.Children.Add(new TextBlock { Text = "Destination Country" });
            var destinationBox = new TextBox { Margin = new Thickness(0, 4, 0, 8) };
            stack.Children.Add(destinationBox);

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

                // Look up country info from the API if a destination was entered
                if (!string.IsNullOrWhiteSpace(destinationBox.Text))
                {
                    CountryInfo info = CountryService.GetCountryInfo(destinationBox.Text.Trim());
                    if (info != null)
                    {
                        trip.Destination    = info.CommonName;
                        trip.FlagUrl        = info.FlagUrl;
                        trip.CurrencyCode   = info.CurrencyCode;
                    }
                    else
                    {
                        trip.Destination = destinationBox.Text.Trim();
                    }
                }

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

            // Clear the edit form so a previously selected item doesn't stay visible
            editingItineraryItem = null;
            ClearItineraryForm();
        }

        private void ItineraryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ItineraryListView.SelectedItem as ItineraryItem;
            if (item == null) return;

            editingItineraryItem = item;
            ItineraryTitleBox.Text = item.Title;
            ItineraryTypeCombo.SelectedIndex = (int)item.ItemType;
            ItineraryDatePicker.SelectedDate = item.Date;
            ItineraryTimeBox.Text = item.Time.ToString(@"hh\:mm");
        }

        private void AddItineraryItem_Click(object sender, RoutedEventArgs e)
        {
            editingItineraryItem = null;
            ClearItineraryForm();
        }

        private void SaveItineraryItem_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTrip == null)
            {
                MessageBox.Show("Please select a trip first.", "No Trip", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ItineraryTitleBox.Text) ||
                ItineraryTypeCombo.SelectedIndex < 0 ||
                !ItineraryDatePicker.SelectedDate.HasValue ||
                !TimeSpan.TryParse(ItineraryTimeBox.Text, out TimeSpan time))
            {
                MessageBox.Show("Please fill in all fields. Time format: HH:mm", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var type = (ItineraryItemType)ItineraryTypeCombo.SelectedIndex;

            if (editingItineraryItem != null)
            {
                editingItineraryItem.Title = ItineraryTitleBox.Text.Trim();
                editingItineraryItem.ItemType = type;
                editingItineraryItem.Date = ItineraryDatePicker.SelectedDate.Value;
                editingItineraryItem.Time = time;
            }
            else
            {
                var item = new ItineraryItem(ItineraryTitleBox.Text.Trim(),
                    ItineraryDatePicker.SelectedDate.Value, time, type);
                item.TripId = SelectedTrip.Id;
                SelectedTrip.ItineraryItems.Add(item);
            }

            db.SaveChanges();
            ClearItineraryForm();

            // Remember which day was selected before refresh resets the list
            int selectedDayIndex = DaysListBox.SelectedIndex;

            // Reload the trip's items from the database so the list is up to date
            db.Entry(SelectedTrip).Collection(t => t.ItineraryItems).Load();

            RefreshTripLists();

            // Restore the day selection so the items list refreshes correctly
            if (selectedDayIndex >= 0)
            {
                DaysListBox.SelectedIndex = selectedDayIndex;
            }
        }

        private void CancelItineraryItem_Click(object sender, RoutedEventArgs e)
        {
            editingItineraryItem = null;
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

            editingBooking = booking;
            BookingReferenceBox.Text = booking.Reference;
            BookingTypeCombo.SelectedIndex = (int)booking.BookingType;
            BookingDatePicker.SelectedDate = booking.CheckInDate;
            BookingFromBox.Text = booking.From;
            BookingToBox.Text = booking.To;
        }

        private void AddBooking_Click(object sender, RoutedEventArgs e)
        {
            editingBooking = null;
            ClearBookingForm();
        }

        private void SaveBooking_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTrip == null)
            {
                MessageBox.Show("Please select a trip first.", "No Trip", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (BookingTypeCombo.SelectedIndex < 0 || !BookingDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Please fill in at least the type and date.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var type = (BookingType)BookingTypeCombo.SelectedIndex;
            string title = !string.IsNullOrWhiteSpace(BookingFromBox.Text) && !string.IsNullOrWhiteSpace(BookingToBox.Text)
                ? $"{BookingFromBox.Text.Trim()} to {BookingToBox.Text.Trim()}"
                : BookingReferenceBox.Text.Trim();

            if (editingBooking != null)
            {
                editingBooking.Title = title;
                editingBooking.Reference = BookingReferenceBox.Text.Trim();
                editingBooking.BookingType = type;
                editingBooking.CheckInDate = BookingDatePicker.SelectedDate.Value;
                editingBooking.Date = BookingDatePicker.SelectedDate.Value;
                editingBooking.From = BookingFromBox.Text.Trim();
                editingBooking.To = BookingToBox.Text.Trim();
            }
            else
            {
                var booking = new Booking(title, BookingReferenceBox.Text.Trim(), type,
                    BookingDatePicker.SelectedDate.Value, null,
                    BookingFromBox.Text.Trim(), BookingToBox.Text.Trim());
                booking.TripId = SelectedTrip.Id;
                SelectedTrip.Bookings.Add(booking);
            }

            db.SaveChanges();
            ClearBookingForm();
            RefreshTripLists();
        }

        private void CancelBooking_Click(object sender, RoutedEventArgs e)
        {
            editingBooking = null;
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

            editingExpense = expense;
            ExpenseCategoryCombo.SelectedIndex = (int)expense.Category;
            ExpenseTitleBox.Text = expense.Title;
            ExpenseAmountBox.Text = expense.Amount.ToString("F2");
            ExpensePaidCombo.SelectedIndex = expense.IsPaid ? 0 : 1;
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            editingExpense = null;
            ClearExpenseForm();
        }

        private void SaveExpense_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTrip == null)
            {
                MessageBox.Show("Please select a trip first.", "No Trip", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ExpenseTitleBox.Text) ||
                ExpenseCategoryCombo.SelectedIndex < 0 ||
                !decimal.TryParse(ExpenseAmountBox.Text, out decimal amount) ||
                ExpensePaidCombo.SelectedIndex < 0)
            {
                MessageBox.Show("Please fill in all fields with valid values.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var category = (ExpenseCategory)ExpenseCategoryCombo.SelectedIndex;
            bool isPaid = ExpensePaidCombo.SelectedIndex == 0;

            if (editingExpense != null)
            {
                editingExpense.Title = ExpenseTitleBox.Text.Trim();
                editingExpense.Category = category;
                editingExpense.Amount = amount;
                editingExpense.IsPaid = isPaid;
            }
            else
            {
                var expense = new Expense(ExpenseTitleBox.Text.Trim(), category, amount, isPaid);
                expense.TripId = SelectedTrip.Id;
                SelectedTrip.Expenses.Add(expense);
            }

            db.SaveChanges();
            ClearExpenseForm();
            RefreshTripLists();
        }

        private void CancelExpense_Click(object sender, RoutedEventArgs e)
        {
            editingExpense = null;
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
