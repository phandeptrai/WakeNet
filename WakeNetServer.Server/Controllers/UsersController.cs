using Microsoft.AspNetCore.Mvc;
using WakeNetServer.Server.Models;
using WakeNetServer.Server.Repository.Interfaces;

namespace WakeNetServer.Server.Controllers;

// API quản lý tài khoản admin (User).
[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserRepository _users;

    public UsersController(IUserRepository users)
    {
        _users = users;
    }

    [HttpGet]
    public ActionResult<List<User>> GetAll()
    {
        return _users.GetAll();
    }

    [HttpGet("{id}")]
    public ActionResult<User> GetById(string id)
    {
        var u = _users.GetById(id);
        return u is null ? NotFound() : Ok(u);
    }

    [HttpGet("by-username/{username}")]
    public ActionResult<User> GetByUsername(string username)
    {
        var u = _users.GetByUsername(username);
        return u is null ? NotFound() : Ok(u);
    }

    [HttpPost]
    public IActionResult Upsert([FromBody] User user)
    {
        if (string.IsNullOrWhiteSpace(user.Id))
            user.Id = Guid.NewGuid().ToString("N");
        if (string.IsNullOrWhiteSpace(user.Username))
            return BadRequest("User.Username is required.");
        if (string.IsNullOrWhiteSpace(user.PasswordHash))
            return BadRequest("User.PasswordHash is required.");

        _users.Upsert(user);
        return Ok(user);
    }
}

