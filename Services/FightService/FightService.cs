using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Fight;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.FightService
{
  public class FightService : IFightService
  {
    private readonly DataContext context;

    public FightService(DataContext context)
    {
      this.context = context;
    }

    public async Task<ServiceResponse<AttackResultDto>> SkillAttack(SkillAttackDto request)
    {
      var response = new ServiceResponse<AttackResultDto>();
      try
      {
        var attacker = await context.Characters.Include(c => c.Skills).FirstOrDefaultAsync(c => c.Id == request.AttackerId);
        var opponent = await context.Characters.FirstOrDefaultAsync(c => c.Id == request.OpponentId);

        if (attacker is null || opponent is null || attacker.Skills is null)
        {
          throw new Exception("Something fisht is going on here...");
        }

        var skill = attacker.Skills.FirstOrDefault(s => s.Id == request.SkillId);

        if (skill is null)
        {
          response.Success = false;
          response.Message = $"{attacker.Name} dosen't know that skill!";
          return response;
        }

        int damage = skill.Damage + (new Random().Next(attacker.Intelligence));
        damage -= new Random().Next(opponent.Defense);

        if (damage > 0)
        {
          opponent.HitPoints -= damage;

        }

        if (opponent.HitPoints <= 0)
        {
          response.Message = $"{opponent.Name} has been defeated";
        }

        await context.SaveChangesAsync();

        response.Data = new AttackResultDto
        {
          Attacker = attacker.Name,
          Opponent = opponent.Name,
          AttackerHP = attacker.HitPoints,
          OpponentHP = opponent.HitPoints,
          Damage = damage
        };

      }
      catch (Exception ex)
      {
        response.Success = false;
        response.Message = ex.Message;
      }

      return response;
    }

    public async Task<ServiceResponse<AttackResultDto>> WeaponAttack(WeaponAttackDto request)
    {
      var response = new ServiceResponse<AttackResultDto>();
      try
      {
        var attacker = await context.Characters.Include(c => c.Weapon).FirstOrDefaultAsync(c => c.Id == request.AttackerId);
        var opponent = await context.Characters.FirstOrDefaultAsync(c => c.Id == request.OpponentId);

        if (attacker is null || opponent is null || attacker.Weapon is null)
        {
          throw new Exception("Something fisht is going on here...");
        }

        int damage = attacker.Weapon.Damage + (new Random().Next(attacker.Strength));
        damage -= new Random().Next(opponent.Defense);

        if (damage > 0)
        {
          opponent.HitPoints -= damage;

        }

        if (opponent.HitPoints <= 0)
        {
          response.Message = $"{opponent.Name} has been defeated";
        }

        await context.SaveChangesAsync();

        response.Data = new AttackResultDto
        {
          Attacker = attacker.Name,
          Opponent = opponent.Name,
          AttackerHP = attacker.HitPoints,
          OpponentHP = opponent.HitPoints,
          Damage = damage
        };

      }
      catch (Exception ex)
      {
        response.Success = false;
        response.Message = ex.Message;
      }

      return response;
    }
  }
}