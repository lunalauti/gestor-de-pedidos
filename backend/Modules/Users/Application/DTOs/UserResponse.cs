using Backend.Shared.DTOs;

namespace Backend.Modules.Users.Application.DTOs {

    public class LoginResponse {
        public string Token { get; set; }
        public int ExpiresIn { get; set; }
        public UserDto User { get; set; }
    }

    public class RegisterResponse {
        public string Message { get; set; } = string.Empty;
    }
}
