using Microsoft.AspNetCore.Mvc;
using BlogAppApi.Data;
using BlogAppApi.DTOs;
using BlogAppApi.Models;
using BlogAppApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BlogAppApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public AuthController(
            ApplicationDbContext context,
            JwtService jwtService
        )
        {
            _context = context;
            _jwtService = jwtService;
        }

        // REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTo dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@"))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid email format"
                });
            }

            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Password must be at least 6 characters long"
                });
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            if (existingUser != null)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Email already exists"
                });
            }

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = !string.IsNullOrEmpty(dto.Role) ? dto.Role : "User"
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "User Registered Successfully"
            });
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTo dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email);

            bool isPasswordValid = false;
            if (user != null)
            {
                try
                {
                    isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);
                }
                catch
                {
                    // Fallback for plain text passwords created before hashing was implemented
                    isPasswordValid = (user.Password == dto.Password);
                }
            }

            if (user == null || !isPasswordValid)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Invalid Email or Password"
                });
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                success = true,
                // message = "Login Successful",
                token = token,
                role = user.Role
            });
        }

        // PROFILE
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "User not found"
                });
            }

            return Ok(new
            {
                success = true,
                id = user.Id,
                name = user.Name,
                email = user.Email
            });
        }
    }
}