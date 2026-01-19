using ChatApp.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController(IUserService presenceTracker) : ControllerBase
    {
        private readonly IUserService _presenceTracker = presenceTracker;

        [HttpGet("online")]
        public async Task<IActionResult> GetOnlineUsers()
        {
            var users = await _presenceTracker.GetOnlineUsersAsync();
            return Ok(users);
        }
    }
}