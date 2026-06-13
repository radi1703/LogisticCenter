using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticsSystem.Models;
using System.Threading.Tasks;

namespace LogisticsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly LogisticsCenterContext _context; 

        public AuthController(LogisticsCenterContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // SQL търси потребителя базата данни игнорира главни/малки букви
            var user = await _context.SystemUsers
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

            // Ако въобще не намери такъв потребител или парола
            if (user == null)
            {
                return Unauthorized(new { message = "Грешно потребителско име или парола!" });
            }

            
            
            if (user.Username != request.Username || user.Password != request.Password)
            {
                return Unauthorized(new { message = "Грешно потребителско име или парола! Моля, спазвайте главните и малките букви." });
            }

            
            return Ok(new { 
                username = user.Username, 
                fullName = user.FullName,
                role = user.Role 
            });
        }
    }
}