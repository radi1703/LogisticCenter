using System.ComponentModel.DataAnnotations;

namespace LogisticsSystem.Models
{
    public class SystemUser
    {
        [Key]
        public int UserId { get; set; }
        
        
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // Помощен клас DTO, който приема данните от формата за вход
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}