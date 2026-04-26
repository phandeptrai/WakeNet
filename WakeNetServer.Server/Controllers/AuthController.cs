using Microsoft.AspNetCore.Mvc;
using WakeNetServer.Server.Models;
using WakeNetServer.Server.Repository.Interfaces;
using WakeNetServer.Server.Services;

namespace WakeNetServer.Server.Controllers;

// API đăng ký/đăng nhập đơn giản dùng Session.
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private const string SessionUserIdKey = "auth.userId";

    private readonly IUserRepository _users;

    public AuthController(IUserRepository users)
    {
        _users = users;
    }

    public sealed class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public sealed class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public sealed class MeResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "Username and Password are required." });

        if (_users.GetByUsername(req.Username) is not null)
            return Conflict(new { message = "Username already exists." });

        var user = new User
        {
            Id = Guid.NewGuid().ToString("N"),
            Username = req.Username.Trim(),
            PasswordHash = PasswordHasher.Hash(req.Password)
        };

        _users.Upsert(user);
        HttpContext.Session.SetString(SessionUserIdKey, user.Id);

        return Ok(new MeResponse { Id = user.Id, Username = user.Username });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "Username and Password are required." });

        var user = _users.GetByUsername(req.Username.Trim());
        if (user is null) return Unauthorized(new { message = "Invalid username or password." });

        if (!PasswordHasher.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid username or password." });

        HttpContext.Session.SetString(SessionUserIdKey, user.Id);
        return Ok(new MeResponse { Id = user.Id, Username = user.Username });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove(SessionUserIdKey);
        return Ok();
    }

    [HttpGet("me")]
    public ActionResult<MeResponse> Me()
    {
        var userId = HttpContext.Session.GetString(SessionUserIdKey);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized(new { message = "Not logged in." });

        var user = _users.GetById(userId);
        if (user is null) return Unauthorized(new { message = "Not logged in." });

        return Ok(new MeResponse { Id = user.Id, Username = user.Username });
    }
}

