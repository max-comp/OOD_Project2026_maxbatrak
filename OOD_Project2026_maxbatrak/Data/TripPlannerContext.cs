using System.Data.Entity;
using System.Data.Entity;
using OOD_Project2026_maxbatrak.Models;

namespace OOD_Project2026_maxbatrak.Data
{

    // Entity Framework Code First DbContext.
    //Represents the database session and maps model classes to tables.
    public class TripPlannerContext : DbContext
    {
        public TripPlannerContext()
            : base("name=TripPlannerDb")
        {
            // Drop and recreate the database when the model changes
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<TripPlannerContext>());
        }

        public DbSet<Trip> Trips { get; set; }
        public DbSet<ItineraryItem> ItineraryItems { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Expense> Expenses { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Trip ? ItineraryItems  (one-to-many)
            modelBuilder.Entity<Trip>()
                .HasMany(t => t.ItineraryItems)
                .WithRequired()
                .HasForeignKey(i => i.TripId)
                .WillCascadeOnDelete(true);

            // Trip ? Bookings  (one-to-many)
            modelBuilder.Entity<Trip>()
                .HasMany(t => t.Bookings)
                .WithRequired()
                .HasForeignKey(b => b.TripId)
                .WillCascadeOnDelete(true);

            // Trip ? Expenses  (one-to-many)
            modelBuilder.Entity<Trip>()
                .HasMany(t => t.Expenses)
                .WithRequired()
                .HasForeignKey(e => e.TripId)
                .WillCascadeOnDelete(true);
        }
    }
}
