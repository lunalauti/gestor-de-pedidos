using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Backend.Modules.Users.Application.DTOs;
using Backend.Modules.Users.Domain.Entities;
using Backend.Modules.Users.Infrastructure.Persistence;
using Backend.Modules.Users.Application.Interfaces;
using Backend.Shared.Services;
using Backend.Shared.DTOs;


namespace Backend.Modules.Users.Application.Services {

    public class AuthService
    {
        private readonly UsersDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordService _passwordService;
        private readonly IUserQueries _userQueries;
        private readonly IRoleQueries _roleQueries;

        public AuthService(UsersDbContext context,
                           IConfiguration configuration,
                           PasswordService passwordService,
                           IUserQueries userQueries,
                           IRoleQueries roleQueries)
        {
            _context = context;
            _configuration = configuration;
            _passwordService = passwordService;
            _userQueries = userQueries;
            _roleQueries = roleQueries;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userQueries.GetUserEntityByEmailAsync(request.Email);
            if (user == null)
            {
                Console.WriteLine($"Login failed: User with email {request.Email} not found.");
                return null;
            }

            // Console.WriteLine($"Verifying password for user {user.Email}, PasswordHash: {user.PasswordHash ?? "NULL"}");
            if (!_passwordService.VerifyPassword(user, user.PasswordHash, request.Password))
            {
                Console.WriteLine($"Login failed: Invalid password for user {user.Email}.");
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.Name)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new LoginResponse
            {
                Token = tokenHandler.WriteToken(token),
                ExpiresIn = 3600,
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Role = new RoleDto
                    {
                        Id = user.Role.Id,
                        Name = user.Role.Name,
                        Description = user.Role.Description
                    }
                }
            };
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _userQueries.ExistsByEmailAsync(request.Email))
                return new RegisterResponse
                {
                    Message = "User already exists"
                };

            var user = new User(request.Name,request.Email);
            var hashed = _passwordService.HashPassword(user, request.Password);
            user.SetPassword(hashed);

            var defaultRole = 2; // User

            var role = request.AdminId.HasValue ? 1 : defaultRole; // Admin

            user.RoleId = role;

            // TODO: Pasar la logica de creacion de usuarios a un factory
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new RegisterResponse
            {
                Message = "User registered successfully"
            };
        }
    }
}
