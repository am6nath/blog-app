using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogAppApi.Data;

namespace BlogAppApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // VIEW ALL USERS
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Email,
                    x.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        // DELETE USER
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                return NotFound("User not found");
            }

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "User deleted successfully"
            });
        }

        // VIEW ALL BLOGS
        [HttpGet("blogs")]
        public async Task<IActionResult> GetAllBlogs()
        {
            var blogs = await _context.Blogs
                .Include(x => x.User)
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.Content,
                    Author = x.User.Name
                })
                .ToListAsync();

            return Ok(blogs);
        }

        // DELETE ANY BLOG
        [HttpDelete("blogs/{id}")]
        public async Task<IActionResult> DeleteAnyBlog(int id)
        {
            var blog = await _context.Blogs
                .FirstOrDefaultAsync(x => x.Id == id);

            if (blog == null)
            {
                return NotFound("Blog not found");
            }

            _context.Blogs.Remove(blog);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Blog deleted by admin"
            });
        }
    }
}