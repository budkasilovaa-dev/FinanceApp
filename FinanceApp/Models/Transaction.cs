using System;

namespace FinanceApp.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public TransactionType Type { get; set; }
        public string Description { get; set; } = "";

        public Transaction(Guid id, DateTime date, decimal amount, TransactionType type, string description)
        {
            Id = id;
            Date = date;
            Amount = amount;
            Type = type;
            Description = description;
        }
    }
}
