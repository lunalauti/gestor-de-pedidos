using Microsoft.EntityFrameworkCore;

using Backend.Modules.Users.Domain.Entities;
using Backend.Shared.DTOs;
using Backend.Modules.Users.Infrastructure.Persistence;
using Backend.Modules.Users.Application.Interfaces;

namespace Backend.Modules.Users.Application.Queries
{
    public class UserQueries: IUserQueries
    {
        private readonly UsersDbContext _usersDbContext;
        
        public UserQueries(UsersDbContext usersDbContext)
        {
            _usersDbContext = usersDbContext;
        }

        // Busca un usuario por email, incluyendo su rol.
        public async Task<UserDto> GetByEmailWithRoleAsync(string email) 
        {
            if (string.IsNullOrEmpty(email))
                return null;
            var user = await _usersDbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = new RoleDto
                {
                    Id = user.Role.Id,
                    Name = user.Role.Name,
                    Description = user.Role.Description
                }
            };
        }

        // Comprueba si existe un usuario con este email.
        public async Task<bool> ExistsByEmailAsync(string email) {
            if (string.IsNullOrEmpty(email))
                return false;
            return await _usersDbContext.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<List<UserDto>> GetUsersAsync()
        {
            return await _usersDbContext.Users
                .Include(u => u.Role)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = new RoleDto
                    {
                        Id = u.Role.Id,
                        Name = u.Role.Name,
                        Description = u.Role.Description
                    }
                })
                .ToListAsync();
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _usersDbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = new RoleDto
                {
                    Id = user.Role.Id,
                    Name = user.Role.Name,
                    Description = user.Role.Description
                }
            };
        }

        public async Task<List<UserDto>> GetMultipleByIdAsync(List<int> ids)
        {
            if (ids == null || !ids.Any())
                return new List<UserDto>(); 

            var users = await _usersDbContext.Users
                .Include(u => u.Role)
                .Where(u => ids.Contains(u.Id))
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = new RoleDto
                    {
                        Id = u.Role.Id,
                        Name = u.Role.Name,
                        Description = u.Role.Description
                    }
                })
                .ToListAsync();

            return users;
        }

        public async Task<User?> GetUserEntityByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            return await _usersDbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

    }
}
