using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Data
{
  public class AuthRepository : IAuthRepository
  {
    private readonly DataContext context;

    public AuthRepository(DataContext context)
    {
      this.context = context;
    }
    public Task<ServiceResponse<string>> Login(string username, string password)
    {
      throw new NotImplementedException();
    }

    public async Task<ServiceResponse<int>> Register(User user, string password)
    {
      CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

      user.passwordHash = passwordHash;
      user.passwordSalt = passwordSalt;

      context.Users.Add(user);
      await context.SaveChangesAsync();
      ServiceResponse<int> response = new ServiceResponse<int>();
      response.Data = user.Id;
      return response;
    }

    public Task<bool> UserExists(string username)
    {
      throw new NotImplementedException();
    }

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
      using (var hmac = new System.Security.Cryptography.HMACSHA512())
      {
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
      }
    }
  }
}