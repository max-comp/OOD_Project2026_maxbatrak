using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOD_Project2026_maxbatrak.Models
{
    //A confirmed booking such as a flight, hotel stay, or transport reservation.
    public class Booking : TripItem, ISearchable, IDeletable
    {
        //Foreign key back to Trip
        public int TripId { get; set; }
        public string Reference { get; set; }
        public BookingType BookingType { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public string From { get; set; }
        public string To { get; set; }

        public Booking()
            : base(string.Empty, DateTime.Today)
        {
        }

        public Booking(string title, string reference, BookingType bookingType,
                        DateTime checkIn, DateTime? checkOut, string from, string to)
            : base(title, checkIn)
        {
            Reference = reference;
            BookingType = bookingType;
            CheckInDate = checkIn;
            CheckOutDate = checkOut;
            From = from;
            To = to;
        }

        [NotMapped]
        public override string Summary
        {
            get
            {
                string route = !string.IsNullOrEmpty(From) && !string.IsNullOrEmpty(To)
                    ? $"{From} → {To}"
                    : Title;

                string dates = CheckOutDate.HasValue
                    ? $"{CheckInDate:MMM dd} – {CheckOutDate:MMM dd}"
                    : $"{CheckInDate:ddd, MMM dd}";

                return $"{route}  |  {dates} ({BookingType})";
            }
        }

        [NotMapped]
        public string DateDisplay
        {
            get
            {
                return CheckOutDate.HasValue
                    ? $"{CheckInDate:MMM dd} – {CheckOutDate:MMM dd}"
                    : $"{CheckInDate:ddd, MMM dd}";
            }
        }

        // ISearchable
        public bool MatchesSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return true;
            query = query.ToLower();
            return Title.ToLower().Contains(query) ||
                   (Reference ?? "").ToLower().Contains(query) ||
                   (From ?? "").ToLower().Contains(query) ||
                   (To ?? "").ToLower().Contains(query) ||
                   BookingType.ToString().ToLower().Contains(query);
        }

        // IDeletable
        [NotMapped]
        public string DeleteConfirmationMessage
        {
            get { return $"Delete booking \"{Title}\"?"; }
        }
    }
}
