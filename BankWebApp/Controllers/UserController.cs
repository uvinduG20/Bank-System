using Microsoft.AspNetCore.Mvc;
using BankWebApp.Services;
using BankWebApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankWebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly BankApiService _bankApiService;

        public UserController(BankApiService bankApiService)
        {
            _bankApiService = bankApiService;
        }

        // User Dashboard: Fetch user profile and account summary
        public async Task<IActionResult> UserDashboard()
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Home");
            }

            var userResponse = await _bankApiService.GetUserByUsername(username);
            if (userResponse.IsSuccessful)
            {
                var user = JsonConvert.DeserializeObject<User>(userResponse.Content);

                var accountsResponse = await _bankApiService.GetAllAccounts();
                if (accountsResponse.IsSuccessful)
                {
                    var accounts = JsonConvert.DeserializeObject<List<Account>>(accountsResponse.Content);

                    var userAccount = accounts.FirstOrDefault(a => a.AccountId == user.AccountId);
                    if (userAccount != null)
                    {
                        ViewBag.User = user;
                        ViewBag.Account = userAccount;
                        return View("UserDashboard");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "No account found for the logged-in user.";
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Unable to fetch accounts.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Unable to fetch user details.";
            }

            return View("UserDashboard");
        }

        // Transaction History: Fetch and display transactions for logged-in user
        public async Task<IActionResult> TransactionHistory(string startDate = null, string endDate = null)
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Home");
            }

            var userResponse = await _bankApiService.GetUserByUsername(username);
            if (userResponse.IsSuccessful)
            {
                var user = JsonConvert.DeserializeObject<User>(userResponse.Content);

                var transactionsResponse = await _bankApiService.GetTransactionsByAccountId(user.AccountId.Value);
                if (transactionsResponse.IsSuccessful)
                {
                    var transactions = JsonConvert.DeserializeObject<List<Transaction>>(transactionsResponse.Content);

                    // Filter transactions by date range if provided
                    if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
                    {
                        DateTime start = DateTime.Parse(startDate);
                        DateTime end = DateTime.Parse(endDate).AddDays(1).AddTicks(-1); // Include full end day
                        transactions = transactions.Where(t => t.TransactionDate >= start && t.TransactionDate <= end).ToList();
                    }

                    // Sort transactions by date (most recent first)
                    transactions = transactions.OrderByDescending(t => t.TransactionDate).ToList();

                    ViewBag.Transactions = transactions;
                    return View("TransactionHistory");
                }
            }

            ViewBag.ErrorMessage = "Unable to fetch transactions.";
            return View("TransactionHistory");
        }

        
        public async Task<IActionResult> UserProfile()
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Home");
            }

            var userResponse = await _bankApiService.GetUserByUsername(username);
            if (userResponse.IsSuccessful)
            {
                var user = JsonConvert.DeserializeObject<User>(userResponse.Content);
                ViewBag.User = user;
                return View("UserProfile"); // Ensure this is directing to "UserProfile"
            }

            ViewBag.ErrorMessage = "Unable to fetch user details.";
            return View("UserProfile");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string Email, string Phone, string Address, string Password)
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Home");
            }

            // Fetch user details to get the existing values
            var userResponse = await _bankApiService.GetUserByUsername(username);
            if (userResponse.IsSuccessful)
            {
                var user = JsonConvert.DeserializeObject<User>(userResponse.Content);

                // Update user details
                
                user.Email = Email;
                user.Phone = Phone;
                user.Address = Address;

                // If password field is filled, update it
                if (!string.IsNullOrEmpty(Password))
                {
                    user.Password = Password;
                }

                // Send the updated user data using PUT request
                var updateResponse = await _bankApiService.UpdateUser(user.Username, user);

                if (updateResponse.IsSuccessful)
                {
                    return RedirectToAction("UserProfile");
                }
                else
                {
                    ViewBag.ErrorMessage = "Failed to update profile.";
                    return View("UserProfile");
                }
            }

            ViewBag.ErrorMessage = "Unable to fetch user details.";
            return View("UserProfile");
        }

        public async Task<IActionResult> MoneyTransfer()
        {
            // Fetch the logged-in user's username
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Home");
            }

            // Fetch user details using the username
            var userResponse = await _bankApiService.GetUserByUsername(username);
            if (userResponse.IsSuccessful)
            {
                var user = JsonConvert.DeserializeObject<User>(userResponse.Content);

                // Fetch all accounts and find the user's account
                var accountsResponse = await _bankApiService.GetAllAccounts();
                if (accountsResponse.IsSuccessful)
                {
                    var accounts = JsonConvert.DeserializeObject<List<Account>>(accountsResponse.Content);

                    // Find the logged-in user's account
                    var userAccount = accounts.FirstOrDefault(a => a.AccountId == user.AccountId);

                    if (userAccount != null)
                    {
                        // Fetch all users for the recipient dropdown, excluding the current user
                        var usersResponse = await _bankApiService.GetAllUsers();
                        if (usersResponse.IsSuccessful)
                        {
                            var users = JsonConvert.DeserializeObject<List<User>>(usersResponse.Content);
                            var recipients = users.Where(u => u.UserId != user.UserId).ToList();

                            ViewBag.User = user;
                            ViewBag.Account = userAccount;
                            ViewBag.Recipients = recipients;

                            return View("MoneyTransfer");
                        }
                    }
                }
            }

            ViewBag.ErrorMessage = "Unable to load money transfer page.";
            return View("MoneyTransfer");
        }

        [HttpPost]
        public async Task<IActionResult> SubmitTransfer(int recipientAccountId, decimal amount)
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Home");
            }

            // Fetch sender (current logged-in user) details
            var userResponse = await _bankApiService.GetUserByUsername(username);
            if (userResponse.IsSuccessful)
            {
                var user = JsonConvert.DeserializeObject<User>(userResponse.Content);

                // Fetch sender's account details
                var accountsResponse = await _bankApiService.GetAllAccounts();
                if (accountsResponse.IsSuccessful)
                {
                    var accounts = JsonConvert.DeserializeObject<List<Account>>(accountsResponse.Content);
                    var senderAccount = accounts.FirstOrDefault(a => a.AccountId == user.AccountId);
                    var recipientAccount = accounts.FirstOrDefault(a => a.AccountId == recipientAccountId);

                    if (senderAccount != null && recipientAccount != null)
                    {
                        if (amount > senderAccount.Balance)
                        {
                            ViewBag.ErrorMessage = "Insufficient funds.";
                            return RedirectToAction("MoneyTransfer");
                        }

                        // Create a transaction for the sender (Debited)
                        var senderTransaction = new Transaction
                        {
                            AccountId = senderAccount.AccountId,
                            TransactionType = "Debited",
                            Amount = amount,
                            TransactionDate = DateTime.Now
                        };
                        await _bankApiService.CreateTransaction(senderTransaction);

                        // Create a transaction for the recipient (Credited)
                        var recipientTransaction = new Transaction
                        {
                            AccountId = recipientAccount.AccountId,
                            TransactionType = "Credited",
                            Amount = amount,
                            TransactionDate = DateTime.Now
                        };
                        await _bankApiService.CreateTransaction(recipientTransaction);

                        // Update the balances
                        senderAccount.Balance -= amount;
                        recipientAccount.Balance += amount;
                        await _bankApiService.UpdateAccountBalance(senderAccount.AccountNumber, senderAccount.Balance);
                        await _bankApiService.UpdateAccountBalance(recipientAccount.AccountNumber, recipientAccount.Balance);

                        // Success message
                        TempData["TransactionSuccess"] = true;
                        return RedirectToAction("MoneyTransfer");
                    }
                }
            }

            ViewBag.ErrorMessage = "Transaction failed.";
            return RedirectToAction("MoneyTransfer");
        }
    }
}
