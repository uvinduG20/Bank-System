using BankWebApp.Models; // Add this for the User model
using BankWebApp.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json; // Add this for JSON deserialization
using System.Linq;
using System.Threading.Tasks;

namespace BankWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly BankApiService _bankApiService;

        public HomeController(BankApiService bankApiService)
        {
            _bankApiService = bankApiService;
        }

        // Render the login page
        public IActionResult Login()
        {
            return View();
        }

        // Handle login logic
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Retrieve all users
            var response = await _bankApiService.GetAllUsers();

            if (response.IsSuccessful)
            {
                var users = JsonConvert.DeserializeObject<List<User>>(response.Content); // Deserialize to List<User>

                // Find user by username and password
                var user = users.FirstOrDefault(u => u.Username == username && u.Password == password);

                if (user != null)
                {
                    // Store user details in session and redirect based on role
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("Role", user.Role);

                    if (user.Role == "Admin")
                    {
                        return RedirectToAction("AdminProfile", "Admin");
                    }
                    else
                    {
                        return RedirectToAction("UserDashboard", "User");
                    }
                }
                else
                {
                    ViewBag.Message = "Invalid username or password";
                }
            }
            else
            {
                ViewBag.Message = "Error retrieving users from the server";
            }

            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear session
            return RedirectToAction("Login", "Home"); // Redirect to login page
        }
    }
}
