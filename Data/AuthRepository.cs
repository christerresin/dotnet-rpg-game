using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
      ServiceResponse<int> response = new ServiceResponse<int>();

      if (await UserExists(user.Username))
      {
        response.Success = false;
        response.Message = "User already exists";
        return response;
      }

      CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

      user.passwordHash = passwordHash;
      user.passwordSalt = passwordSalt;

      context.Users.Add(user);
      await context.SaveChangesAsync();
      response.Data = user.Id;
      return response;
    }

    public async Task<bool> UserExists(string username)
    {
      if (await context.Users.AnyAsync(U => U.Username.ToLower() == username.ToLower()))
      {
        return true;
      }

      return false;
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