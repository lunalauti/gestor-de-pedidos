using System.ComponentModel.DataAnnotations;

namespace Backend.Modules.Users.Domain.Entities{
public class User
  {
      public int Id { get; private set; }
      public string Name { get; private set; }
      public string Email { get; private set; }
      public string PasswordHash { get; private set; }

      [Required]
        public int RoleId { get; set; }

      public User(string name, string email)
      {
        Name = name;
        Email = email;
      }
      public void SetRole(Role role) => Role = role;

      public void SetPassword(string passwordHash) => PasswordHash = passwordHash;
      public Role Role { get; private set; }
  }
}