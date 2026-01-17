using System.Net;
using System.Security.Claims;
using ChatApp.DTOs;
using ChatApp.Interfaces.JWT;
using ChatApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(UserManager<ApplicationUser> userManager,
                                IJwtTokenService jwtTokenService) : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IJwtTokenService _jwtTokenService = jwtTokenService;

        [HttpPost("register")]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            var userNameExist = await _userManager.FindByNameAsync(request.Username);
            if (userNameExist != null)
                return BadRequest("Nome de usuário indisponível");

            var emailExists = await _userManager.FindByEmailAsync(request.Email);
            if (emailExists != null)
                return BadRequest("Email já cadastrado");

            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
                return Unauthorized("Usuário ou senha inválidos");

            var isValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isValid)
                return Unauthorized("Usuário ou senha inválidos");

            var roles = await _userManager.GetRolesAsync(user);

            var token = _jwtTokenService.Generate(user, roles);

            return Ok(new { token });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(new
            {
                userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                userName = User.Identity?.Name
            });
        }
    }
}