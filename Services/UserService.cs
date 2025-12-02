using icone_backend.Data;
using icone_backend.Models;
using Microsoft.EntityFrameworkCore;


namespace icone_backend.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserModel?> FindByIdAsync(long id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
