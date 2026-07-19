using System;
using System.Collections.Generic;
using System.Linq;

namespace FinanceApp.Models
{
    public enum TransactionType { Income, Expense }

    public class Wallet
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Currency { get; set; } = "";
        public decimal InitialBalance { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();

        public decimal CurrentBalance => InitialBalance
                                        + Transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount)
                                        - Transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        public Wallet(Guid id, string name, string currency, decimal initial)
        {
            Id = id;
            Name = name;
            Currency = currency;
            InitialBalance = initial;
        }

        public bool TryAddTransaction(Transaction transaction, out string? error)
        {
            error = null;
            if (transaction.Type == TransactionType.Expense && transaction.Amount > CurrentBalance)
            {
                error = $"Недостаточно средств. Баланс: {CurrentBalance:0.00} {Currency}, попытка расхода: {transaction.Amount:0.00} {Currency}";
                return false;
            }

            Transactions.Add(transaction);
            return true;
        }
    }
}
