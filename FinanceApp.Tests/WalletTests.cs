using System;
using Xunit;
using FinanceApp.Models;

namespace FinanceApp.Tests
{
    public class WalletTests
    {
        [Fact]
        public void Wallet_InitialBalance_ShouldBeSetCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            string name = "Тестовый кошелек";
            string currency = "RUB";
            decimal initialBalance = 2000m;

            // Act
            var wallet = new Wallet(id, name, currency, initialBalance);

            // Assert
            Assert.Equal(id, wallet.Id);
            Assert.Equal(name, wallet.Name);
            Assert.Equal(currency, wallet.Currency);
            Assert.Equal(initialBalance, wallet.CurrentBalance);
        }

        [Fact]
        public void AddingIncome_ShouldIncreaseBalance()
        {
            var wallet = new Wallet(Guid.NewGuid(), "Кошелек", "RUB", 1000m);
            var income = new Transaction(Guid.NewGuid(), DateTime.Today, 500m, TransactionType.Income, "Зарплата");

            bool result = wallet.TryAddTransaction(income, out string? error);

            Assert.True(result);
            Assert.Null(error);
            Assert.Equal(1500m, wallet.CurrentBalance);
        }

        [Fact]
        public void AddingExpense_LessThanBalance_ShouldSucceed()
        {
            var wallet = new Wallet(Guid.NewGuid(), "Кошелек", "RUB", 1000m);
            var expense = new Transaction(Guid.NewGuid(), DateTime.Today, 500m, TransactionType.Expense, "Покупка");

            bool result = wallet.TryAddTransaction(expense, out string? error);

            Assert.True(result);
            Assert.Null(error);
            Assert.Equal(500m, wallet.CurrentBalance);
        }

        [Fact]
        public void AddingExpense_MoreThanBalance_ShouldFail()
        {
            var wallet = new Wallet(Guid.NewGuid(), "Кошелек", "RUB", 1000m);
            var expense = new Transaction(Guid.NewGuid(), DateTime.Today, 1500m, TransactionType.Expense, "Покупка");

            bool result = wallet.TryAddTransaction(expense, out string? error);

            Assert.False(result);
            Assert.NotNull(error);
            Assert.Equal(1000m, wallet.CurrentBalance);
        }
    }
}
