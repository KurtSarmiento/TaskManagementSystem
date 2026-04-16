using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskManagementSystem.Models;
using TaskManagementSystem.Utils;

namespace TaskManagementSystem.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    [APIKeyAuthorize]
    [EnableRateLimiting("AdminTasks")]
    public class AdminController : ControllerBase
    {
        private static List<UserModel> Users = new List<UserModel>
        {
            new UserModel { Id = 1, Username = "admin", Role = "Admin" },
            new UserModel { Id = 2, Username = "manager", Role = "Manager" },
            new UserModel { Id = 3, Username = "employee", Role = "Employee" }
        };

        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            return Ok(Users);
        }

        [HttpDelete("users/{id}")]
        public IActionResult DeleteUser(int id)
        {
            var user = Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound("User not found");

            Users.Remove(user);

            return Ok($"User {user.Username} deleted successfully");
        }
    }
}