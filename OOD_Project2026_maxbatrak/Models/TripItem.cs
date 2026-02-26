using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOD_Project2026_maxbatrak.Models
{
    //Abstract base class for all items that belong to a trip.
    // Demonstrates meaningful use of abstraction — every trip-related item
    // has a title, a date, and must provide its own display summary.
    public abstract class TripItem
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }

        [NotMapped]
        public abstract string Summary { get; }

        protected TripItem(string title, DateTime date)
        {
            Title = title;
            Date = date;
        }

        public override string ToString()
        {
            return Summary;
        }
    }
}
