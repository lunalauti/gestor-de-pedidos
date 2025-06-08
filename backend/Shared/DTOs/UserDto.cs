namespace Backend.Shared.DTOs {
    public class UserDto {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public RoleDto Role { get; set; }
    }

    public class RoleDto {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}