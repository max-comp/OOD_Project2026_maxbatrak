using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OOD_Project2026_maxbatrak.Models
{
    //A single expense entry used for budget tracking.
    //Inherits Title and Date from TripItem and adds financial details.

    public class Expense : TripItem, ISearchable, IDeletable
    {
        // Foreign key back to Trip
        public int TripId { get; set; }
        public ExpenseCategory Category { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }

        public Expense()
            : base(string.Empty, DateTime.Today)
        {
        }

        public Expense(string title, ExpenseCategory category, decimal amount, bool isPaid)
            : base(title, DateTime.Today)
        {
            Category = category;
            Amount = amount;
            IsPaid = isPaid;
        }


        [NotMapped]
        public override string Summary
        {
            get
            {
                string status = IsPaid ? "Paid" : "Owed";
                return $"{Title} – {Amount:C} ({Category}) – {status}";
            }
        }

        [NotMapped]
        public string AmountDisplay
        {
            get { return Amount.ToString("C"); }
        }

        [NotMapped]
        public string PaidDisplay
        {
            get { return IsPaid ? "Paid" : "Owed"; }
        }

        // ISearchable
        public bool MatchesSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return true;
            query = query.ToLower();
            return Title.ToLower().Contains(query) ||
                   Category.ToString().ToLower().Contains(query);
        }

        // IDeletable
        [NotMapped]
        public string DeleteConfirmationMessage
        {
            get { return $"Delete expense \"{Title}\" ({Amount:C})?"; }
        }
    }
}
