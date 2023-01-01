using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.User;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly IAuthRepository authRepository;

    public AuthController(IAuthRepository authRepository)
    {
      this.authRepository = authRepository;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ServiceResponse<int>>> Register(UserRegisterDto request)
    {
      var response = await authRepository.Register(new Models.User { Username = request.Username }, request.Password);
      if (!response.Success)
      {
        return BadRequest(response);
      }

      return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<ServiceResponse<string>>> Login(UserLoginDto request)
    {
      var response = await authRepository.Login(request.Username, request.Password);
      if (!response.Success)
      {
        return BadRequest(response);
      }

      return Ok(response);
    }
  }
}