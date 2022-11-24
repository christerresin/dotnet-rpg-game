using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Dtos.Character;
using dotnet_rpg.Services.CharacterService;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{

  [ApiController]
  [Route("api/[controller]")]
  public class CharacterController : ControllerBase
  {
    private readonly ICharacterService characterService;
    public CharacterController(ICharacterService characterService)
    {
      this.characterService = characterService;
    }

    [HttpGet("GetAll")]
    public async Task<ActionResult<ServiceResponse<List<GetCharacterDto>>>> Get()
    {
      return Ok(await characterService.GetAllCharacters());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceResponse<GetCharacterDto>>> GetSingle(int id)
    {
      return Ok(await characterService.GetCharacterById(id));
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse<List<GetCharacterDto>>>> AddCharacter(AddCharacterDto newCharacter)
    {
      return Ok(await characterService.AddCharacter(newCharacter));
    }
    [HttpPut]
    public async Task<ActionResult<ServiceResponse<GetCharacterDto>>> UpdateCharacter(UpdateCharacterDto updatedCharacter)
    {
      var serviceResponse = await characterService.UpdateCharacter(updatedCharacter);
      if (serviceResponse.Data == null)
      {
        return NotFound(serviceResponse);
      }
      return Ok(serviceResponse);
    }
  }
}