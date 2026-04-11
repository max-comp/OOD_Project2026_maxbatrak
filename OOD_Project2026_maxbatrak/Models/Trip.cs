using System;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace OOD_Project2026_maxbatrak.Models
{
    //Represents a single trip.
    //Acts as the top-level container holding all itinerary items, bookings, and expenses for that trip.
    public class Trip : ISearchable, IDeletable
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Populated from the RestCountries API lookup
        public string Destination { get; set; }
        public string FlagUrl { get; set; }
        public string CurrencyCode { get; set; }
 
        //navigation properties — use List<T> so EF can persist them
        public virtual List<ItineraryItem> ItineraryItems { get; set; }
        public virtual List<Booking> Bookings { get; set; }
        public virtual List<Expense> Expenses { get; set; }

        public Trip()
        {
            ItineraryItems = new List<ItineraryItem>();
            Bookings       = new List<Booking>();
            Expenses       = new List<Expense>();
        }

        public Trip(string name, DateTime startDate, DateTime endDate)
            : this()
        {
            Name      = name;
            StartDate = startDate;
            EndDate   = endDate;
        }

        [NotMapped]
        public decimal TotalExpenses
        {
            get { return Expenses.Sum(e => e.Amount); }
        }

        [NotMapped]
        public decimal TotalPaid
        {
            get { return Expenses.Where(e => e.IsPaid).Sum(e => e.Amount); }
        }

        [NotMapped]
        public decimal TotalOwed
        {
            get { return Expenses.Where(e => !e.IsPaid).Sum(e => e.Amount); }
        }

        public ObservableCollection<DateTime> GetTripDays()
        {
            var days = new ObservableCollection<DateTime>();
            for (DateTime d = StartDate.Date; d <= EndDate.Date; d = d.AddDays(1))
            {
                days.Add(d);
            }
            return days;
        }

        public ObservableCollection<ItineraryItem> GetItemsForDay(DateTime day)
        {
            var items = ItineraryItems
                .Where(i => i.Date.Date == day.Date)
                .OrderBy(i => i.Time);

            return new ObservableCollection<ItineraryItem>(items);
        }

        public override string ToString()
        {
            return $"{Name} ({StartDate:MMM dd} – {EndDate:MMM dd})";
        }

        // ISearchable
        public bool MatchesSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return true;
            return (Name ?? "").ToLower().Contains(query.ToLower())
                || (Destination ?? "").ToLower().Contains(query.ToLower());
        }

        // IDeletable
        [NotMapped]
        public string DeleteConfirmationMessage
        {
            get { return $"Delete trip \"{Name}\" and all its items?"; }
        }
    }
}
