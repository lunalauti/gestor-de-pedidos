using Backend.Shared.DTOs;
using Backend.Modules.Users.Domain.Entities;

namespace Backend.Modules.Users.Application.Interfaces {
    public interface IUserQueries
    {
        Task<List<UserDto>> GetUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto> GetByEmailWithRoleAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
        Task<List<UserDto>> GetMultipleByIdAsync(List<int> ids);

        // Solo para uso interno !! (no exponer en la API)
        Task<User?> GetUserEntityByEmailAsync(string email);


    }
}
