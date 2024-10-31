using BankWebService.Data;
using BankWebService.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace BankWebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        // Create a new transaction
        [HttpPost]
        public IActionResult CreateTransaction([FromBody] Transaction transaction)
        {
            // Input validation
            if (transaction == null || transaction.AccountId <= 0 ||
                string.IsNullOrWhiteSpace(transaction.TransactionType) ||
                transaction.Amount <= 0)
            {
                return BadRequest("Invalid transaction data. Ensure all fields are filled correctly.");
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.connectionString))
                {
                    connection.Open();
                    string insertQuery = "INSERT INTO Transactions (AccountId, TransactionType, Amount, TransactionDate) VALUES (@AccountId, @TransactionType, @Amount, @TransactionDate)";
                    using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@AccountId", transaction.AccountId);
                        command.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
                        command.Parameters.AddWithValue("@Amount", transaction.Amount);
                        command.Parameters.AddWithValue("@TransactionDate", transaction.TransactionDate.ToString("o"));
                        command.ExecuteNonQuery();
                    }
                }
                return CreatedAtAction(nameof(GetTransaction), new { id = transaction.TransactionId }, transaction);
            }
            catch (SQLiteException ex)
            {
                // Log the error (not implemented here for simplicity)
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log the error (not implemented here for simplicity)
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        // Retrieve transaction by ID
        [HttpGet("{id}")]
        public IActionResult GetTransaction(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid transaction ID.");
            }

            try
            {
                Transaction transaction = null;
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.connectionString))
                {
                    connection.Open();
                    string selectQuery = "SELECT * FROM Transactions WHERE TransactionId = @TransactionId";
                    using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@TransactionId", id);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                transaction = new Transaction
                                {
                                    TransactionId = reader.GetInt32(0),
                                    AccountId = reader.GetInt32(1),
                                    TransactionType = reader.GetString(2),
                                    Amount = reader.GetDecimal(3),
                                    TransactionDate = DateTime.Parse(reader.GetString(4))
                                };
                            }
                        }
                    }
                }
                if (transaction == null) return NotFound($"Transaction with ID {id} not found.");
                return Ok(transaction);
            }
            catch (SQLiteException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        // Retrieve transaction history for a specific account
        [HttpGet("account/{accountId}")]
        public IActionResult GetTransactionHistory(int accountId)
        {
            if (accountId <= 0)
            {
                return BadRequest("Invalid account ID.");
            }

            try
            {
                var transactions = new List<Transaction>();
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.connectionString))
                {
                    connection.Open();
                    string selectQuery = "SELECT * FROM Transactions WHERE AccountId = @AccountId";
                    using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@AccountId", accountId);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(new Transaction
                                {
                                    TransactionId = reader.GetInt32(0),
                                    AccountId = reader.GetInt32(1),
                                    TransactionType = reader.GetString(2),
                                    Amount = reader.GetDecimal(3),
                                    TransactionDate = DateTime.Parse(reader.GetString(4))
                                });
                            }
                        }
                    }
                }
                return Ok(transactions);
            }
            catch (SQLiteException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        // Retrieve all transactions
        [HttpGet]
        public IActionResult GetAllTransactions()
        {
            try
            {
                var transactions = new List<Transaction>();
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.connectionString))
                {
                    connection.Open();
                    string selectQuery = "SELECT * FROM Transactions";
                    using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                transactions.Add(new Transaction
                                {
                                    TransactionId = reader.GetInt32(0),
                                    AccountId = reader.GetInt32(1),
                                    TransactionType = reader.GetString(2),
                                    Amount = reader.GetDecimal(3),
                                    TransactionDate = DateTime.Parse(reader.GetString(4))
                                });
                            }
                        }
                    }
                }
                return Ok(transactions);
            }
            catch (SQLiteException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }
    }
}
