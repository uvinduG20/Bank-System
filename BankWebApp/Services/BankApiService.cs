using RestSharp;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BankWebApp.Models;
using System;
using Microsoft.AspNetCore.Hosting;

namespace BankWebApp.Services
{
    public class BankApiService
    {
        private readonly RestClient _client;
        private readonly string logFilePath;

        // Constructor to accept IWebHostEnvironment for dynamic log path handling
        public BankApiService(IWebHostEnvironment environment)
        {
            _client = new RestClient("http://localhost:5055/api/");

            // Set the log directory path dynamically based on the app's root folder
            string logDirectory = Path.Combine(environment.ContentRootPath, "Logs");

            // Ensure the Logs directory exists
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Set the full path to the log file
            logFilePath = Path.Combine(logDirectory, "AdminLogs.txt");
        }

        // Get all users
        public async Task<RestResponse> GetAllUsers()
        {
            try
            {
                var request = new RestRequest("users", Method.Get);
                var response = await _client.ExecuteAsync(request);
                //LogApiResponse("GetAllUsers", response);
                return response;
            }
            catch (Exception ex)
            {
                LogException("GetAllUsers", ex);
                throw;
            }
        }

        // Get all accounts
        public async Task<RestResponse> GetAllAccounts()
        {
            try
            {
                var request = new RestRequest("accounts", Method.Get);
                var response = await _client.ExecuteAsync(request);
                //LogApiResponse("GetAllAccounts", response);
                return response;
            }
            catch (Exception ex)
            {
                LogException("GetAllAccounts", ex);
                throw;
            }
        }

        // Get all transactions
        public async Task<RestResponse> GetAllTransactions()
        {
            try
            {
                var request = new RestRequest("transactions", Method.Get);
                var response = await _client.ExecuteAsync(request);
                //LogApiResponse("GetAllTransactions", response);
                return response;
            }
            catch (Exception ex)
            {
                LogException("GetAllTransactions", ex);
                throw;
            }
        }

        // Get a user by username
        public async Task<RestResponse> GetUserByUsername(string username)
        {
            try
            {
                var request = new RestRequest($"users/{username}", Method.Get);
                var response = await _client.ExecuteAsync(request);
                //LogApiResponse($"GetUserByUsername ({username})", response);
                return response;
            }
            catch (Exception ex)
            {
                LogException($"GetUserByUsername ({username})", ex);
                throw;
            }
        }

        // Get an account by AccountId
        public async Task<RestResponse> GetAccountById(int accountId)
        {
            try
            {
                var request = new RestRequest($"accounts/{accountId}", Method.Get);
                var response = await _client.ExecuteAsync(request);
                //LogApiResponse($"GetAccountById ({accountId})", response);
                return response;
            }
            catch (Exception ex)
            {
                LogException($"GetAccountById ({accountId})", ex);
                throw;
            }
        }

        // Get transactions by account ID
        public async Task<RestResponse> GetTransactionsByAccountId(int accountId)
        {
            try
            {
                var request = new RestRequest($"transactions/account/{accountId}", Method.Get);
                var response = await _client.ExecuteAsync(request);
                //LogApiResponse($"GetTransactionsByAccountId ({accountId})", response);
                return response;
            }
            catch (Exception ex)
            {
                LogException($"GetTransactionsByAccountId ({accountId})", ex);
                throw;
            }
        }

        // Create a new user
        public async Task<RestResponse> CreateUser(User newUser)
        {
            try
            {
                var request = new RestRequest("users", Method.Post);
                request.AddJsonBody(newUser);
                var response = await _client.ExecuteAsync(request);
                //LogApiResponse("CreateUser", response);
                return response;
            }
            catch (Exception ex)
            {
                LogException("CreateUser", ex);
                throw;
            }
        }

        // Update a user
        public async Task<RestResponse> UpdateUser(string username, User updatedUser)
        {
            try
            {
                var request = new RestRequest($"users/{username}", Method.Put);
                request.AddJsonBody(updatedUser); // Ensure the correct data is being passed in the body
                var response = await _client.ExecuteAsync(request);
                //LogApiResponse($"UpdateUser ({username})", response);
                return response;
            }
            catch (Exception ex)
            {
                LogException($"UpdateUser ({username})", ex);
                throw;
            }
        }

        // Create a transaction
        public async Task<RestResponse> CreateTransaction(Transaction transaction)
        {
            try
            {
                var request = new RestRequest("transactions", Method.Post);
                request.AddJsonBody(transaction);
                var response = await _client.ExecuteAsync(request);
                //LogApiResponse("CreateTransaction", response);
                return response;
            }
            catch (Exception ex)
            {
                LogException("CreateTransaction", ex);
                throw;
            }
        }

        // Update account balance
        public async Task<RestResponse> UpdateAccountBalance(string accountNumber, decimal newBalance)
        {
            try
            {
                var request = new RestRequest($"accounts/{accountNumber}", Method.Put);
                request.AddJsonBody(new { AccountNumber = accountNumber, Balance = newBalance });
                var response = await _client.ExecuteAsync(request);
                //LogApiResponse($"UpdateAccountBalance ({accountNumber})", response);
                return response;
            }
            catch (Exception ex)
            {
                LogException($"UpdateAccountBalance ({accountNumber})", ex);
                throw;
            }
        }

        // Delete a user
        public async Task<RestResponse> DeleteUser(string username)
        {
            try
            {
                var request = new RestRequest($"users/{username}", Method.Delete);
                var response = await _client.ExecuteAsync(request);
                //LogApiResponse($"DeleteUser ({username})", response);
                return response;
            }
            catch (Exception ex)
            {
                LogException($"DeleteUser ({username})", ex);
                throw;
            }
        }

        // Save log entry to a file
        public void SaveLog(LogEntry logEntry)
        {
            // Ensure log timestamps are in local time if that's preferred
            string log = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {logEntry.Action} - {logEntry.Details}";
            File.AppendAllText(logFilePath, log + Environment.NewLine);  // Append log to file
        }





        // Get logs from file
        public List<string> GetSecurityLogs()
        {
            if (File.Exists(logFilePath))
            {
                return new List<string>(File.ReadAllLines(logFilePath)); // Read logs from file
            }

            return new List<string> { "No logs found." };
        }

        // Utility method for logging API responses
        private void LogApiResponse(string action, RestResponse response)
        {
            string log = $"API Action: {action}, Status: {(response.IsSuccessful ? "Success" : "Failure")}, " +
                         $"Status Code: {response.StatusCode}, Message: {response.ErrorMessage}";
            SaveLog(new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Action = action,
                Details = log
            });
        }

        // Utility method for logging exceptions
        private void LogException(string action, Exception ex)
        {
            string log = $"Exception during {action}: {ex.Message}";
            SaveLog(new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Action = action,
                Details = log
            });
        }


    }
}
