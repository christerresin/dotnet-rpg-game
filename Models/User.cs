using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace dotnet_rpg.Models
{
  public class User
  {
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public byte[] passwordHash { get; set; } = new Byte[0];
    public byte[] passwordSalt { get; set; } = new Byte[0];
    public List<Character>? Characters { get; set; }
  }
}