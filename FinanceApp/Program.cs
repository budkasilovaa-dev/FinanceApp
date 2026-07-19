using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using FinanceApp.Models;

namespace FinanceApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("Финансовый учёт — консольное приложение");

            var wallets = DataProvider.ChooseAndLoadWallets();

            Console.Write("Введите месяц и год в формате YYYY-MM: ");
            var input = Console.ReadLine();
            if (!DateTime.TryParseExact(input + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var month))
            {
                Console.WriteLine("Неверный формат даты");
                return;
            }

            var allTransactions = wallets.SelectMany(w => w.Transactions.Select(t => (Wallet: w, Transaction: t)))
                .Where(x => x.Transaction.Date.Year == month.Year && x.Transaction.Date.Month == month.Month)
                .ToList();

            if (!allTransactions.Any())
            {
                Console.WriteLine("Транзакций не найдено");
                return;
            }

            var grouped = allTransactions
                .GroupBy(x => x.Transaction.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    Total = g.Sum(x => x.Transaction.Amount),
                    Transactions = g.OrderBy(x => x.Transaction.Date).ToList()
                })
                .OrderByDescending(g => g.Total)
                .ToList();

            Console.WriteLine($"\nТранзакции за {month:yyyy-MM}:");
            foreach (var grp in grouped)
            {
                Console.WriteLine($"\n{grp.Type} — сумма: {grp.Total:0.00}");
                foreach (var item in grp.Transactions)
                {
                    var t = item.Transaction;
                    Console.WriteLine($"{t.Date:yyyy-MM-dd} | {item.Wallet.Name,-10} | {t.Amount,8:0.00} {item.Wallet.Currency} | {t.Description}");
                }
            }

            Console.WriteLine("\nТоп-3 расходов за месяц:");
            foreach (var w in wallets)
            {
                var topExpenses = w.Transactions
                    .Where(t => t.Type == TransactionType.Expense && t.Date.Year == month.Year && t.Date.Month == month.Month)
                    .OrderByDescending(t => t.Amount)
                    .Take(3)
                    .ToList();

                Console.WriteLine($"\n{w.Name} ({w.Currency}) — баланс: {w.CurrentBalance:0.00}");
                if (!topExpenses.Any())
                {
                    Console.WriteLine("  Нет расходов");
                    continue;
                }

                int i = 1;
                foreach (var t in topExpenses)
                {
                    Console.WriteLine($"  {i}. {t.Date:yyyy-MM-dd} — {t.Amount,8:0.00} {w.Currency} — {t.Description}");
                    i++;
                }
            }
        }
    }

    static class DataProvider
    {
        public static List<Wallet> ChooseAndLoadWallets()
        {
            Console.WriteLine("1 — Сгенерировать тестовые данные");
            Console.WriteLine("2 — Прочитать из файла data.json");
            Console.WriteLine("3 — Ввести вручную");
            Console.Write("Выбор: ");
            var choice = Console.ReadLine();

            return choice switch
            {
                "2" => LoadFromFile("data.json"),
                "3" => ManualInput(),
                _ => GenerateSampleData(),
            };
        }

        public static List<Wallet> LoadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("Файл не найден, создаются тестовые данные");
                return GenerateSampleData();
            }

            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var wallets = JsonSerializer.Deserialize<List<Wallet>>(json, options);
            return wallets ?? new List<Wallet>();
        }

        public static List<Wallet> ManualInput()
        {
            var wallets = new List<Wallet>();
            while (true)
            {
                Console.Write("Имя кошелька (Enter — закончить): ");
                var name = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name)) break;

                Console.Write("Валюта: ");
                var currency = Console.ReadLine();
                Console.Write("Начальный баланс: ");
                decimal.TryParse(Console.ReadLine(), out var balance);

                var wallet = new Wallet(Guid.NewGuid(), name ?? "", currency ?? "", balance);

                // Добавление транзакций с проверкой баланса
                while (true)
                {
                    Console.Write("Добавить транзакцию? (y/n): ");
                    var addTx = Console.ReadLine();
                    if (addTx?.ToLower() != "y") break;

                    Console.Write("Дата (yyyy-MM-dd): ");
                    DateTime.TryParse(Console.ReadLine(), out var date);

                    Console.Write("Сумма: ");
                    decimal.TryParse(Console.ReadLine(), out var amount);

                    Console.Write("Тип (Income/Expense): ");
                    var typeInput = Console.ReadLine();
                    var type = typeInput?.ToLower() == "income" ? TransactionType.Income : TransactionType.Expense;

                    Console.Write("Описание: ");
                    var description = Console.ReadLine() ?? "";

                    var tx = new Transaction(Guid.NewGuid(), date, amount, type, description);
                    if (!wallet.TryAddTransaction(tx, out var error))
                        Console.WriteLine(error);
                }

                wallets.Add(wallet);
            }
            return wallets;
        }

        public static List<Wallet> GenerateSampleData()
        {
            var now = DateTime.Now;
            var year = now.Year;
            var month = now.Month;

            var w1 = new Wallet(Guid.NewGuid(), "Кошелёк", "RUB", 10000m);
            var w2 = new Wallet(Guid.NewGuid(), "Карта", "RUB", 50000m);

            w1.TryAddTransaction(new Transaction(Guid.NewGuid(), new DateTime(year, month, 2), 1500m, TransactionType.Expense, "Одежда"), out _);
            w1.TryAddTransaction(new Transaction(Guid.NewGuid(), new DateTime(year, month, 5), 2000m, TransactionType.Income, "Зарплата"), out _);
            w1.TryAddTransaction(new Transaction(Guid.NewGuid(), new DateTime(year, month, 10), 300m, TransactionType.Expense, "Обед"), out _);

            w2.TryAddTransaction(new Transaction(Guid.NewGuid(), new DateTime(year, month, 3), 12000m, TransactionType.Expense, "Ремонт техники"), out _);
            w2.TryAddTransaction(new Transaction(Guid.NewGuid(), new DateTime(year, month, 8), 4000m, TransactionType.Expense, "Телевизор"), out _);
            w2.TryAddTransaction(new Transaction(Guid.NewGuid(), new DateTime(year, month, 15), 30000m, TransactionType.Income, "Зарплата"), out _);

            var wallets = new List<Wallet> { w1, w2 };

            // Сохраняем в data.json
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText("data.json", JsonSerializer.Serialize(wallets, options));

            return wallets;
        }
    }
}
