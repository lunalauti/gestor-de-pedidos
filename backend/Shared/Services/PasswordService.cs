using Microsoft.AspNetCore.Identity;
using Backend.Modules.Users.Domain.Entities;

namespace Backend.Shared.Services
{
    public class PasswordService
    {
        private readonly PasswordHasher<User> _passwordHasher;

        public PasswordService()
        {
            _passwordHasher = new PasswordHasher<User>();
        }

        public string HashPassword(User user, string password)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));

            var hashedPassword = _passwordHasher.HashPassword(user, password);
            return hashedPassword;
        }

        public bool VerifyPassword(User user, string hashedPassword, string passwordToCheck)
        {
            if (user == null)
            {
                Console.WriteLine("Error: User is null.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(hashedPassword))
            {
                Console.WriteLine("Error: hashedPassword is null or empty.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(passwordToCheck))
            {
                Console.WriteLine("Error: passwordToCheck is null or empty.");
                return false;
            }

            var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, passwordToCheck);
            return result != PasswordVerificationResult.Failed;
        }
    }
}