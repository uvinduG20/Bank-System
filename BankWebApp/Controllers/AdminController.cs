using Microsoft.AspNetCore.Mvc;
using BankWebApp.Services;
using BankWebApp.Models;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BankWebApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly BankApiService _bankApiService;

        public AdminController(IWebHostEnvironment environment)
        {
            _bankApiService = new BankApiService(environment);
        }

        // Admin Dashboard: Fetch admin profile and display information
        public async Task<IActionResult> AdminProfile()
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Home");
            }

            var userResponse = await _bankApiService.GetUserByUsername(username);
            if (userResponse.IsSuccessful)
            {
                var admin = JsonConvert.DeserializeObject<User>(userResponse.Content);
                ViewBag.Admin = admin;
                return View("AdminProfile");
            }

            ViewBag.ErrorMessage = "Unable to fetch admin details.";
            return View("AdminProfile");
        }

        // Handle profile update for admin
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> UpdateAdminProfile(string Email, string Phone, string Address, string Password)
        {
            var username = HttpContext.Session.GetString("Username");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Home");
            }

            var adminResponse = await _bankApiService.GetUserByUsername(username);
            if (adminResponse.IsSuccessful)
            {
                var admin = JsonConvert.DeserializeObject<User>(adminResponse.Content);
                admin.Email = Email;
                admin.Phone = Phone;
                admin.Address = Address;

                if (!string.IsNullOrEmpty(Password))
                {
                    admin.Password = Password;
                }

                var updateResponse = await _bankApiService.UpdateUser(admin.Username, admin);
                if (updateResponse.IsSuccessful)
                {
                    // Log the profile update
                    _bankApiService.SaveLog(new LogEntry
                    {
                        AdminUsername = HttpContext.Session.GetString("Username"),
                        Action = "UpdateProfile",
                        Details = $"Updated profile for {username}"
                    });

                    // Redirect to the admin profile view instead of returning JSON
                    return RedirectToAction("AdminProfile", "Admin");
                }
                else
                {
                    ViewBag.ErrorMessage = "Failed to update profile.";
                    return View("AdminProfile");
                }
            }

            ViewBag.ErrorMessage = "Unable to fetch admin details.";
            return View("AdminProfile");
        }

        public async Task<IActionResult> UserManagement()
        {
            var response = await _bankApiService.GetAllUsers();
            if (response.IsSuccessful)
            {
                var users = JsonConvert.DeserializeObject<List<User>>(response.Content);
                ViewBag.Users = users;
                return View();
            }
            return View("Error");
        }

        public async Task<IActionResult> GetAllUsers()
        {
            var response = await _bankApiService.GetAllUsers();
            if (response.IsSuccessful)
            {
                var users = JsonConvert.DeserializeObject<List<User>>(response.Content);
                return Json(new { success = true, users = users });
            }

            return Json(new { success = false });
        }

        public async Task<IActionResult> GetUserByUsername(string username)
        {
            var response = await _bankApiService.GetUserByUsername(username);
            if (response.IsSuccessful)
            {
                var user = JsonConvert.DeserializeObject<User>(response.Content);
                return Json(new { success = true, user = user });
            }

            return Json(new { success = false, message = "Failed to load user details." });
        }

        // Create New User
        [HttpPost]
        public async Task<IActionResult> CreateUser(string Name, string Username, string Email, string Phone, string Address, string Picture, string Password, string Role)
        {
            try
            {
                var usersResponse = await _bankApiService.GetAllUsers();
                if (usersResponse.IsSuccessful)
                {
                    var users = JsonConvert.DeserializeObject<List<User>>(usersResponse.Content);
                    int nextUserId = users.Max(u => u.UserId) + 1;
                    int nextAccountId = users.Max(u => u.AccountId ?? 0) + 1;

                    var newUser = new User
                    {
                        UserId = nextUserId,
                        AccountId = nextAccountId,
                        Name = Name,
                        Username = Username,
                        Email = Email,
                        Address = Address,
                        Phone = Phone,
                        Picture = Picture,
                        Password = Password,
                        Role = Role
                    };

                    var createUserResponse = await _bankApiService.CreateUser(newUser);
                    if (createUserResponse.IsSuccessful)
                    {
                        // Log creation action
                        _bankApiService.SaveLog(new LogEntry
                        {
                            AdminUsername = HttpContext.Session.GetString("Username"),
                            Action = "CreateUser",
                            Details = $"Created user {Username}"
                        });

                        return Json(new { success = true, message = "User created successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to create user." });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Failed to retrieve users." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Edit User
        [HttpPost]
        public async Task<IActionResult> UpdateUser(string Username, string Email, string Phone, string Address, string Role, string Password, string Picture)
        {
            var userResponse = await _bankApiService.GetUserByUsername(Username);
            if (userResponse.IsSuccessful)
            {
                var user = JsonConvert.DeserializeObject<User>(userResponse.Content);

                // Update the user fields
                user.Email = Email;
                user.Phone = Phone;
                user.Address = Address;
                user.Role = Role;
                user.Picture = Picture;

                if (!string.IsNullOrEmpty(Password))
                {
                    user.Password = Password;
                }

                // Send the updated user data to the API
                var updateResponse = await _bankApiService.UpdateUser(user.Username, user);
                Console.WriteLine($"Response Status: {updateResponse.StatusCode}, Response: {updateResponse.Content}");

                if (updateResponse.IsSuccessful)
                {
                    // Log the update action
                    _bankApiService.SaveLog(new LogEntry
                    {
                        AdminUsername = HttpContext.Session.GetString("Username"),
                        Action = "UpdateUser",
                        Details = $"Updated user {Username}"
                    });

                    return Json(new { success = true, message = "User updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to update user." });
                }
            }

            return Json(new { success = false, message = "Failed to load user details." });
        }




        [HttpPost]
        public async Task<IActionResult> DeleteUser(string username)
        {
            // Call the API to delete the user
            var response = await _bankApiService.DeleteUser(username);
            Console.WriteLine($"Response Status: {response.StatusCode}, Response: {response.Content}");

            // Check if the API call was successful
            if (response.IsSuccessful)
            {
                // Log the deletion action
                _bankApiService.SaveLog(new LogEntry
                {
                    AdminUsername = HttpContext.Session.GetString("Username"),
                    Action = "DeleteUser",
                    Details = $"Deleted user {username}"
                });

                return Json(new { success = true, message = "User deleted successfully!" });
            }
            else
            {
                // Log the error details for debugging
                _bankApiService.SaveLog(new LogEntry
                {
                    AdminUsername = HttpContext.Session.GetString("Username"),
                    Action = "DeleteUser",
                    Details = $"Failed to delete user {username}. API Response: {response.Content}"
                });

                return Json(new { success = false, message = $"Failed to delete user. Server response: {response.Content}" });
            }
        }




        public async Task<IActionResult> TransactionManagement()
        {
            var response = await _bankApiService.GetAllTransactions();
            if (response.IsSuccessful)
            {
                var transactions = JsonConvert.DeserializeObject<List<Transaction>>(response.Content);
                ViewBag.Transactions = transactions;
                return View();
            }

            ViewBag.ErrorMessage = "Failed to load transactions.";
            return View("Error");
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactionsByAccountId(int accountId)
        {
            var response = await _bankApiService.GetTransactionsByAccountId(accountId);
            if (response.IsSuccessful)
            {
                var transactions = JsonConvert.DeserializeObject<List<Transaction>>(response.Content);
                return Json(new { success = true, transactions = transactions });
            }

            return Json(new { success = false, message = "Failed to load transactions for the given account ID." });
        }

        public async Task<IActionResult> SecurityLogs()
        {
            var logs = _bankApiService.GetSecurityLogs();
            ViewBag.Logs = logs;
            return View("SecurityLogs");
        }
    }
}
