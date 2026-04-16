using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskManagementSystem.Models;
using TaskManagementSystem.Utils;

namespace TaskManagementSystem.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService = new JwtService();
        [HttpPost("Login")]
        [APIKeyAuthorize]
        [EnableRateLimiting("LoginPolicy")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if(model.Username == "admin")
            {
                var token = _jwtService.GenerateToken(model.Username, "Admin");
                return Ok(new { token });
            }
            else if (model.Username == "manager")
            {
                var token = _jwtService.GenerateToken(model.Username, "Manager");
                return Ok(new { token });
            }
            else
            {
                var token = _jwtService.GenerateToken(model.Username, "Employee");
                return Ok(new { token });
            }
        }
    }
}
