using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskManagementSystem.Utils;

namespace TaskManagementSystem.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Employee")]
        [APIKeyAuthorize]
        [EnableRateLimiting("GetTasks")]
        public IActionResult GetTasks() => Ok("List of Taskszxczczxcz");

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [EnableRateLimiting("PostTasks")]
        public IActionResult CreateTask() => Ok("Task created");
    }
}
