namespace Backend.Modules.Users.Application.DTOs {
    
    public class LoginRequest {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int? AdminId { get; set; }
    }
}


