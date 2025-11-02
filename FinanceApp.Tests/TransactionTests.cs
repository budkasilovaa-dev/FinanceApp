using System;
using Xunit;
using FinanceApp.Models;

namespace FinanceApp.Tests
{
    public class TransactionTests
    {
        [Fact]
        public void Transaction_Creation_ShouldSetCorrectProperties()
        {
            // Arrange
            var id = Guid.NewGuid();
            var date = DateTime.Today;
            decimal amount = 100m;
            var type = TransactionType.Expense;
            string description = "Тест";

            // Act
            var transaction = new Transaction(id, date, amount, type, description);

            // Assert
            Assert.Equal(id, transaction.Id);
            Assert.Equal(amount, transaction.Amount);
            Assert.Equal(type, transaction.Type);
            Assert.Equal(description, transaction.Description);
            Assert.Equal(date, transaction.Date);
        }
    }
}
