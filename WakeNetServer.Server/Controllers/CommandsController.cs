using Microsoft.AspNetCore.Mvc;
using WakeNetServer.Server.Models;
using WakeNetServer.Server.Repository.Interfaces;

namespace WakeNetServer.Server.Controllers;

// API tạo/lấy lệnh và nhận kết quả thực thi lệnh (Command/CommandResult).
[ApiController]
[Route("api/[controller]")]
public sealed class CommandsController : ControllerBase
{
    private readonly ICommandRepository _commands;

    public CommandsController(ICommandRepository commands)
    {
        _commands = commands;
    }

    [HttpPost]
    public IActionResult Create([FromBody] Command command)
    {
        if (string.IsNullOrWhiteSpace(command.MachineId))
            return BadRequest("Command.MachineId is required.");

        if (string.IsNullOrWhiteSpace(command.Id))
            command.Id = Guid.NewGuid().ToString("N");

        if (command.CreatedAt == default)
            command.CreatedAt = DateTime.UtcNow;

        if (command.Status == default)
            command.Status = CommandStatus.Pending;

        _commands.Add(command);
        return Ok(command);
    }

    [HttpGet("pending/{machineId}")]
    public ActionResult<List<Command>> GetPending(string machineId)
    {
        return _commands.GetPendingByMachineId(machineId);
    }

    [HttpPost("result")]
    public IActionResult SaveResult([FromBody] CommandResult result)
    {
        if (string.IsNullOrWhiteSpace(result.CommandId))
            return BadRequest("CommandResult.CommandId is required.");
        if (string.IsNullOrWhiteSpace(result.MachineId))
            return BadRequest("CommandResult.MachineId is required.");
        if (result.ExecutedAt == default)
            result.ExecutedAt = DateTime.UtcNow;

        _commands.SaveResult(result);

        var cmd = _commands.GetById(result.CommandId);
        if (cmd is not null)
        {
            cmd.Status = result.Success ? CommandStatus.Executed : CommandStatus.Failed;
            _commands.Update(cmd);
        }

        return Ok();
    }
}

