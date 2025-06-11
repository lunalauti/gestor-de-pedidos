using Backend.Shared.DTOs;


namespace Users.Application.Interfaces {
    public interface IRoleQueries
    {
        Task<RoleDto> GetByIdAsync(int id);
        Task<List<RoleDto>> GetRolesAsync();
    }
}
