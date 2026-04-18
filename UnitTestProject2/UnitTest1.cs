using Microsoft.VisualStudio.TestTools.UnitTesting;
using OOD_Project2026_maxbatrak.Models;
using System;

namespace UnitTestProject2
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Trip_TotalExpenses_ReturnsSumOfAllAmounts()
        {
            var trip = new Trip("Holiday", new DateTime(2025, 6, 1), new DateTime(2025, 6, 7));
            trip.Expenses.Add(new Expense("Hotel", ExpenseCategory.Lodging, 100m, true));
            trip.Expenses.Add(new Expense("Dinner", ExpenseCategory.Dining, 40m, false));

            Assert.AreEqual(140m, trip.TotalExpenses);
        }

        [TestMethod]
        public void Trip_TotalPaid_OnlyCountsPaidExpenses()
        {
            var trip = new Trip("Holiday", new DateTime(2025, 6, 1), new DateTime(2025, 6, 7));
            trip.Expenses.Add(new Expense("Hotel", ExpenseCategory.Lodging, 100m, true));
            trip.Expenses.Add(new Expense("Dinner", ExpenseCategory.Dining, 40m, false));

            Assert.AreEqual(100m, trip.TotalPaid);
        }

       
    }
}
