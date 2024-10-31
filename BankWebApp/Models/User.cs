namespace BankWebApp.Models
{
    public class User
    {
        public int UserId { get; set; }
        public int? AccountId { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Picture { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
