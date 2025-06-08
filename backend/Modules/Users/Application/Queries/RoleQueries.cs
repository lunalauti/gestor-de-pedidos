using Microsoft.EntityFrameworkCore;

using Backend.Modules.Users.Domain.Entities;
using Backend.Shared.DTOs;
using Backend.Modules.Users.Infrastructure.Persistence;
using Backend.Modules.Users.Application.Interfaces;

namespace Backend.Modules.Users.Application.Queries
{
    public class RoleQueries : IRoleQueries
    {
        private readonly UsersDbContext _usersDbContext;

        public RoleQueries(UsersDbContext usersDbContext)
        {
            _usersDbContext = usersDbContext;
        }

        public async Task<RoleDto> GetByIdAsync(int id)
        {
            var role = await _usersDbContext.Roles
                .FirstOrDefaultAsync(r => r.Id == id);
            
            if (role == null) return null;

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description
            };
        }

        public async Task<List<RoleDto>> GetRolesAsync()
        {
            return await _usersDbContext.Roles
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description
                })
                .ToListAsync();
        }
    }
}