using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogAppApi.Data;
using BlogAppApi.DTOs;
using BlogAppApi.Models;
using System.Security.Claims;

namespace BlogAppApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BlogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // VIEW ALL BLOGS
        [HttpGet]
        public async Task<IActionResult> GetBlogs()
        {
            var blogs = await _context.Blogs
                .AsNoTracking()
                .Include(x => x.User)
                .Select(x => new
                {
                    id = x.Id,
                    title = x.Title,
                    content = x.Content,
                    author = x.User.Name
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                blogs = blogs
            });
        }

        // ADD BLOG
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddBlog(AddBlogDTo dto)
        {
            // VALIDATION
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Title is required"
                });
            }

            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Content is required"
                });
            }

            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            var blog = new Blog
            {
                Title = dto.Title,
                Content = dto.Content,
                UserId = userId
            };

            _context.Blogs.Add(blog);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Blog added successfully",
                blog = new
                {
                    id = blog.Id,
                    title = blog.Title,
                    content = blog.Content
                }
            });
        }

        // VIEW MY BLOGS
        [HttpGet("myblogs")]
        [Authorize]
        public async Task<IActionResult> MyBlogs()
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            var blogs = await _context.Blogs
                .Where(x => x.UserId == userId)
                .Select(x => new
                {
                    id = x.Id,
                    title = x.Title,
                    content = x.Content
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                blogs = blogs
            });
        }
        // READ MORE BLOG
        [HttpGet("details/{id}")]
        public async Task<IActionResult> ReadMoreBlog(int id)
        {
            var blog = await _context.Blogs
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (blog == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Blog not found"
                });
            }

            return Ok(new
            {
                success = true,
                blog = new
                {
                    id = blog.Id,
                    title = blog.Title,
                    content = blog.Content,
                    author = blog.User.Name
                }
            });
        }

        // GET SINGLE BLOG
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetSingleBlog(int id)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId
                );

            if (blog == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Blog not found"
                });
            }

            return Ok(new
            {
                success = true,
                blog = new
                {
                    id = blog.Id,
                    title = blog.Title,
                    content = blog.Content
                }
            });
        }

        // EDIT BLOG
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> EditBlog(
            int id,
            UpdateBlogDTo dto
        )
        {
            // VALIDATION
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Title is required"
                });
            }

            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Content is required"
                });
            }

            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(x => x.Id == id);

            if (blog == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Blog not found"
                });
            }

            // CHECK OWNERSHIP
            if (blog.UserId != userId)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "You are not allowed to edit this blog"
                });
            }

            blog.Title = dto.Title;
            blog.Content = dto.Content;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Blog updated successfully",
                blog = new
                {
                    id = blog.Id,
                    title = blog.Title,
                    content = blog.Content
                }
            });
        }

        // DELETE BLOG
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)
            );

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(x => x.Id == id);

            if (blog == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Blog not found"
                });
            }

            // CHECK OWNERSHIP
            if (blog.UserId != userId)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "You are not allowed to delete this blog"
                });
            }

            _context.Blogs.Remove(blog);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Blog deleted successfully"
            });
        }
    }
}