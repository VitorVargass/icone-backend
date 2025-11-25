using icone_backend.Data;
using icone_backend.Dtos.Admins;
using icone_backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;



namespace icone_backend.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class AdminsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var emailExists = await _context.Admins
                .AnyAsync(a => a.Email == request.Email);

            if (emailExists)
                return Conflict(new { message = "Email já está sendo usado por outro admin." });

            var admin = new AdminModel
            {
                Name = request.Name,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = admin.Id }, new
            {
                admin.Id,
                admin.Name,
                admin.Email,
                admin.Role,
                admin.IsActive,
                admin.CreatedAt
            });
        }
        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var admin = await _context.Admins
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (admin is null)
                return NotFound();

            return Ok(new
            {
                admin.Id,
                admin.Name,
                admin.Email,
                admin.Role,
                admin.IsActive,
                admin.CreatedAt
            });
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateAdminRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Id == id);
            if (admin is null)
                return NotFound(new { message = "Admin não encontrado." });

            
            if (!string.Equals(admin.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailExists = await _context.Admins
                    .AnyAsync(a => a.Email == request.Email && a.Id != id);

                if (emailExists)
                    return Conflict(new { message = "Email já está sendo usado por outro admin." });
            }

            admin.Name = request.Name;
            admin.Email = request.Email;
            admin.Role = request.Role;
            admin.IsActive = request.IsActive;

            
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            _context.Admins.Update(admin);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Admin atualizado com sucesso.",
                admin = new
                {
                    admin.Id,
                    admin.Name,
                    admin.Email,
                    admin.Role,
                    admin.IsActive,
                    admin.CreatedAt
                }
            });
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Id == id);
            if (admin is null)
                return NotFound(new { message = "Admin não encontrado." });

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
