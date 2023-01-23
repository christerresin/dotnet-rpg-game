global using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Character;

namespace dotnet_rpg.Services.CharacterService
{
  public class CharacterService : ICharacterService
  {
    private readonly IMapper mapper;
    private readonly DataContext context;
    private readonly IHttpContextAccessor httpContextAccessor;

    public CharacterService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
    {
      this.httpContextAccessor = httpContextAccessor;
      this.mapper = mapper;
      this.context = context;
    }

    private int GetUserId() => int.Parse(httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<ServiceResponse<List<GetCharacterDto>>> AddCharacter(AddCharacterDto newCharacter)
    {
      var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
      Character character = mapper.Map<Character>(newCharacter);
      character.User = await context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());
      context.Characters.Add(character);
      await context.SaveChangesAsync();
      serviceResponse.Data = await context.Characters
        .Where(c => c.User!.Id == GetUserId())
        .Select(c => mapper.Map<GetCharacterDto>(c))
        .ToListAsync();
      return serviceResponse;
    }

    public async Task<ServiceResponse<GetCharacterDto>> DeleteCharacter(int id)
    {
      ServiceResponse<GetCharacterDto> serviceResponse = new ServiceResponse<GetCharacterDto>();

      try
      {
        var character = await context.Characters
        .FirstAsync(c => c.Id == id && c.User!.Id == GetUserId());
        context.Characters.Remove(character);
        await context.SaveChangesAsync();
        serviceResponse.Data = mapper.Map<GetCharacterDto>(character);
      }
      catch (Exception ex)
      {
        serviceResponse.Success = false;
        serviceResponse.Message = ex.Message;
      }
      return serviceResponse;
    }

    public async Task<ServiceResponse<List<GetCharacterDto>>> GetAllCharacters()
    {
      var serviceResponse = new ServiceResponse<List<GetCharacterDto>>();
      var dbCharacters = await context.Characters
      .Include(c => c.Weapon)
      .Include(c => c.Skills)
      .Where(c => c.User!.Id == GetUserId()).ToListAsync();
      serviceResponse.Data = dbCharacters.Select(c => mapper.Map<GetCharacterDto>(c)).ToList();
      return serviceResponse;
    }

    public async Task<ServiceResponse<GetCharacterDto>> GetCharacterById(int id)
    {
      var serviceResponse = new ServiceResponse<GetCharacterDto>();
      var dbCharacter = await context.Characters
      .Include(c => c.Weapon)
      .Include(c => c.Skills)
      .FirstOrDefaultAsync(c => c.Id == id && c.User!.Id == GetUserId());
      serviceResponse.Data = mapper.Map<GetCharacterDto>(dbCharacter);
      return serviceResponse;
    }

    public async Task<ServiceResponse<GetCharacterDto>> UpdateCharacter(UpdateCharacterDto updatedCharacter)
    {
      ServiceResponse<GetCharacterDto> serviceResponse = new ServiceResponse<GetCharacterDto>();

      try
      {
        var character = await context.Characters
        .Include(c => c.User)
        .FirstOrDefaultAsync(c => c.Id == updatedCharacter.Id);

        if (character is null || character.User!.Id != GetUserId())
        {
          throw new Exception($"Character with id '{updatedCharacter.Id}' not found.");
        }

        character.Name = updatedCharacter.Name;
        character.HitPoints = updatedCharacter.HitPoints;
        character.Strength = updatedCharacter.Strength;
        character.Defense = updatedCharacter.Defense;
        character.Intelligence = updatedCharacter.Intelligence;
        character.Class = updatedCharacter.Class;

        await context.SaveChangesAsync();

        serviceResponse.Data = mapper.Map<GetCharacterDto>(character);
      }
      catch (Exception ex)
      {
        serviceResponse.Success = false;
        serviceResponse.Message = ex.Message;
      }
      return serviceResponse;
    }

    public async Task<ServiceResponse<GetCharacterDto>> AddCharacterSkill(AddCharacterSkillDto newCharacterSkill)
    {
      var response = new ServiceResponse<GetCharacterDto>();

      try
      {
        var character = await context.Characters
        .Include(c => c.Weapon)
        .Include(c => c.Skills)
        .FirstOrDefaultAsync(c => c.Id == newCharacterSkill.CharacterId &&
        c.User!.Id == GetUserId());

        if (character is null)
        {
          response.Success = false;
          response.Message = "Character not found.";
          return response;
        }

        var skill = await context.Skills.FirstOrDefaultAsync(s => s.Id == newCharacterSkill.SkillId);

        if (skill is null)
        {
          response.Success = false;
          response.Message = "Skill not found.";
          return response;
        }

        character.Skills!.Add(skill);
        await context.SaveChangesAsync();
        response.Data = mapper.Map<GetCharacterDto>(character);

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