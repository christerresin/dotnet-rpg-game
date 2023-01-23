using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace dotnet_rpg.Data
{
  public class AuthRepository : IAuthRepository
  {
    private readonly DataContext context;
    private IConfiguration configuration { get; }

    // Need access to appsettings.json - get it throu IConfiguration
    public AuthRepository(DataContext context, IConfiguration configuration)
    {
      this.configuration = configuration;
      this.context = context;
    }
    public async Task<ServiceResponse<string>> Login(string username, string password)
    {
      var response = new ServiceResponse<string>();
      var user = await context.Users.FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()));

      if (user == null)
      {
        response.Success = false;
        response.Message = "User not found.";
      }
      else if (!VerifyPasswordHash(password, user.passwordHash, user.passwordSalt))
      {
        response.Success = false;
        response.Message = "Wrong password.";
      }
      else
      {
        response.Data = CreateToken(user);
      }

      return response;
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
      if (await context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))
      {
        return true;
      }

      return false;
    }

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
      using (var hmac = new System.Security.Cryptography.HMACSHA512())
      {
        // creating an instance of hmac generates a key. We use that key as salt
        passwordSalt = hmac.Key;
        // GetBytes to turn password string into byte array. Then hashing the password + salt
        passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
      }
    }

    private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
      // creating an instance of hmac but with param for salt, if left w/o param it auto generates a key
      using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
      {
        var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return computeHash.SequenceEqual(passwordHash);
      }
    }

    private string CreateToken(User user)
    {
      // Create a list of Claims for the Jwt
      var claims = new List<Claim>
      {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username)
      };

      // getting the Token key from appsettings.json (injected IConfiguration configuration)
      var appSettingsToken = configuration.GetSection("AppSettings:Token").Value;
      if (appSettingsToken is null)
      {
        throw new Exception("AppSettings Token is null!");
      }

      SymmetricSecurityKey key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(appSettingsToken));

      SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

      var tokenDescriptor = new SecurityTokenDescriptor { Subject = new ClaimsIdentity(claims), Expires = DateTime.Now.AddMinutes(15), SigningCredentials = creds };

      JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
      SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

      // tokenHandler.WriteToken serializes the token to a string
      return tokenHandler.WriteToken(token);

    }
  }
}