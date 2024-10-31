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
    public class AccountsController : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateAccount([FromBody] Account account)
        {
            // Validate input
            if (account == null || string.IsNullOrWhiteSpace(account.AccountNumber) || account.Balance < 0)
            {
                return BadRequest("Invalid account data. Ensure all fields are filled correctly.");
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.connectionString))
                {
                    connection.Open();

                    // Insert query modified to include AccountId
                    string insertQuery = "INSERT INTO Accounts (AccountId, AccountNumber, Balance) VALUES (@AccountId, @AccountNumber, @Balance)";
                    using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@AccountId", account.AccountId);
                        command.Parameters.AddWithValue("@AccountNumber", account.AccountNumber);
                        command.Parameters.AddWithValue("@Balance", account.Balance);
                        command.ExecuteNonQuery();
                    }
                }
                return CreatedAtAction(nameof(GetAccount), new { accountNumber = account.AccountNumber }, account);
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

        [HttpGet("{accountNumber}")]
        public IActionResult GetAccount(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return BadRequest("Account number cannot be empty.");
            }

            try
            {
                Account account = null;
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.connectionString))
                {
                    connection.Open();
                    string selectQuery = "SELECT * FROM Accounts WHERE AccountNumber = @AccountNumber";
                    using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                account = new Account
                                {
                                    AccountId = reader.GetInt32(0),
                                    AccountNumber = reader.GetString(1),
                                    Balance = reader.GetDecimal(2)
                                };
                            }
                        }
                    }
                }
                if (account == null) return NotFound($"Account with number {accountNumber} not found.");
                return Ok(account);
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

        [HttpPut("{accountNumber}")]
        public IActionResult UpdateAccount(string accountNumber, [FromBody] Account updatedAccount)
        {
            if (string.IsNullOrWhiteSpace(accountNumber) || updatedAccount == null || updatedAccount.Balance < 0)
            {
                return BadRequest("Invalid account update data. Ensure all fields are filled correctly.");
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Accounts SET Balance = @Balance WHERE AccountNumber = @AccountNumber";
                    using (SQLiteCommand command = new SQLiteCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Balance", updatedAccount.Balance);
                        command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound($"Account with number {accountNumber} not found.");
                        }
                    }
                }
                return Ok($"Account with number {accountNumber} has been successfully updated.");
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


        [HttpDelete("{accountNumber}")]
        public IActionResult DeleteAccount(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return BadRequest("Account number cannot be empty.");
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.connectionString))
                {
                    connection.Open();
                    // Use LOWER to make the comparison case-insensitive
                    string deleteQuery = "DELETE FROM Accounts WHERE LOWER(AccountNumber) = LOWER(@AccountNumber)";
                    using (SQLiteCommand command = new SQLiteCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return NotFound($"Account with number {accountNumber} not found.");
                        }
                    }
                }
                return Ok($"Account with number {accountNumber} has been successfully deleted.");
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



        [HttpGet]
        public IActionResult GetAllAccounts()
        {
            try
            {
                var accounts = new List<Account>();
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.connectionString))
                {
                    connection.Open();
                    string selectQuery = "SELECT * FROM Accounts";
                    using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                accounts.Add(new Account
                                {
                                    AccountId = reader.GetInt32(0),
                                    AccountNumber = reader.GetString(1),
                                    Balance = reader.GetDecimal(2)
                                });
                            }
                        }
                    }
                }
                return Ok(accounts);
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
