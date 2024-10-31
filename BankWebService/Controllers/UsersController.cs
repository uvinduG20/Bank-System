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
    public class UsersController : ControllerBase
    {
        // Create a new user
        [HttpPost]
        public IActionResult CreateUser([FromBody] User user)
        {
            // Input validation
            if (user == null || user.UserId <= 0 || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrWhiteSpace(user.Role))
            {
                return BadRequest("Invalid user data. UserId, Name, Username, Email, and Role are required.");
            }

            // Validate the role
            if (user.Role != "Admin" && user.Role != "User")
            {
                return BadRequest("Invalid role. Must be 'Admin' or 'User'.");
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.connectionString))
                {
                    connection.Open();
                    string insertQuery = "INSERT INTO Users (UserId, AccountId, Name, Username, Email, Address, Phone, Picture, Password, Role) " +
                                         "VALUES (@UserId, @AccountId, @Name, @Username, @Email, @Address, @Phone, @Picture, @Password, @Role)";
                    using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", user.UserId); // New UserId parameter
                        command.Parameters.AddWithValue("@AccountId", user.AccountId);
                        command.Parameters.AddWithValue("@Name", user.Name);
                        command.Parameters.AddWithValue("@Username", user.Username);
                        command.Parameters.AddWithValue("@Email", user.Email);
                        command.Parameters.AddWithValue("@Address", user.Address);
                        command.Parameters.AddWithValue("@Phone", user.Phone);
                        command.Parameters.AddWithValue("@Picture", user.Picture);
                        command.Parameters.AddWithValue("@Password", user.Password);
                        command.Parameters.AddWithValue("@Role", user.Role);
                        command.ExecuteNonQuery();
                    }
                }
                return CreatedAtAction(nameof(GetUser), new { username = user.Username }, user);
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



        // Retrieve user by username
        [HttpGet("{username}")]
        public IActionResult GetUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest("Invalid username.");
            }

            try
            {
                User user = null;
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.connectionString))
                {
                    connection.Open();
                    string selectQuery = "SELECT * FROM Users WHERE Username = @Username";
                    using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new User
                                {
                                    UserId = reader.GetInt32(0),
                                    AccountId = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                    Name = reader.GetString(2),
                                    Username = reader.GetString(3),
                                    Email = reader.GetString(4),
                                    Address = reader.GetString(5),
                                    Phone = reader.GetString(6),
                                    Picture = reader.GetString(7),
                                    Password = reader.GetString(8),
                                    Role = reader.GetString(9) // Retrieve role
                                };
                            }
                        }
                    }
                }
                if (user == null) return NotFound($"User with username '{username}' not found.");
                return Ok(user);
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

        // Update user details
        [HttpPut("{username}")]
        public IActionResult UpdateUser(string username, [FromBody] User updatedUser)
        {
            if (string.IsNullOrWhiteSpace(username) || updatedUser == null || string.IsNullOrWhiteSpace(updatedUser.Name) || string.IsNullOrWhiteSpace(updatedUser.Role))
            {
                return BadRequest("Invalid username or user data.");
            }

            // Validate the role
            if (updatedUser.Role != "Admin" && updatedUser.Role != "User")
            {
                return BadRequest("Invalid role. Must be 'Admin' or 'User'.");
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Users SET Name = @Name, Email = @Email, Address = @Address, Phone = @Phone, Picture = @Picture, Password = @Password, Role = @Role WHERE Username = @Username";
                    using (SQLiteCommand command = new SQLiteCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Name", updatedUser.Name);
                        command.Parameters.AddWithValue("@Email", updatedUser.Email);
                        command.Parameters.AddWithValue("@Address", updatedUser.Address);
                        command.Parameters.AddWithValue("@Phone", updatedUser.Phone);
                        command.Parameters.AddWithValue("@Picture", updatedUser.Picture);
                        command.Parameters.AddWithValue("@Password", updatedUser.Password);
                        command.Parameters.AddWithValue("@Role", updatedUser.Role); // Update role
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            return NotFound($"User with username '{username}' not found.");
                        }
                    }
                }
                return NoContent();
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

        // Delete user
        [HttpDelete("{username}")]
        public IActionResult DeleteUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest("Invalid username.");
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.connectionString))
                {
                    connection.Open();
                    string deleteQuery = "DELETE FROM Users WHERE Username = @Username";
                    using (SQLiteCommand command = new SQLiteCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            return NotFound($"User with username '{username}' not found.");
                        }
                    }
                }
                return NoContent();
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

        // Retrieve all users
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            try
            {
                var users = new List<User>();
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.connectionString))
                {
                    connection.Open();
                    string selectQuery = "SELECT * FROM Users";
                    using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new User
                                {
                                    UserId = reader.GetInt32(0),
                                    AccountId = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                    Name = reader.GetString(2),
                                    Username = reader.GetString(3),
                                    Email = reader.GetString(4),
                                    Address = reader.GetString(5),
                                    Phone = reader.GetString(6),
                                    Picture = reader.GetString(7),
                                    Password = reader.GetString(8),
                                    Role = reader.GetString(9) // Retrieve role
                                });
                            }
                        }
                    }
                }
                return Ok(users);
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
