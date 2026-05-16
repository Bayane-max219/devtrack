using System.Security.Claims;
using DevTrack.API.DTOs;
using DevTrack.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(IUserRepository userRepo) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var user = await userRepo.GetByIdAsync(CurrentUserId);
        if (user is null) return NotFound();

        return Ok(new UserProfileDto(
            user.Id, user.FirstName, user.LastName,
            user.Email, user.Role.ToString(), user.CreatedAt
        ));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName))
            return BadRequest(new { message = "FirstName and LastName are required" });

        var user = await userRepo.GetByIdAsync(CurrentUserId);
        if (user is null) return NotFound();

        user.FirstName = req.FirstName.Trim();
        user.LastName = req.LastName.Trim();

        await userRepo.UpdateAsync(user);

        return Ok(new UserProfileDto(
            user.Id, user.FirstName, user.LastName,
            user.Email, user.Role.ToString(), user.CreatedAt
        ));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await userRepo.GetAllAsync();
        var result = users.Select(u => new UserSummaryDto(
            u.Id,
            u.FirstName + " " + u.LastName,
            u.Email
        ));
        return Ok(result);
    }
}
