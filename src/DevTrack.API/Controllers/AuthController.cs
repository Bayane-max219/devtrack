using DevTrack.API.DTOs;
using DevTrack.API.Services;
using DevTrack.Domain.Entities;
using DevTrack.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DevTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IUserRepository userRepo, TokenService tokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var existing = await userRepo.GetByEmailAsync(req.Email);
        if (existing != null)
            return BadRequest(new { error = "Email déjà utilisé" });

        var user = new User
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            Email = req.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };

        await userRepo.AddAsync(user);
        var token = tokenService.GenerateToken(user);

        return Ok(new AuthResponse(token, user.Email, user.Role.ToString(), user.Id));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await userRepo.GetByEmailAsync(req.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { error = "Email ou mot de passe incorrect" });

        var token = tokenService.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Email, user.Role.ToString(), user.Id));
    }
}
