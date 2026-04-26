using Microsoft.AspNetCore.Mvc;
using WakeNetServer.Server.Models;
using WakeNetServer.Server.Repository.Interfaces;

namespace WakeNetServer.Server.Controllers;

// API quản lý danh sách máy client (Machine).
[ApiController]
[Route("api/[controller]")]
public sealed class MachinesController : ControllerBase
{
    private readonly IMachineRepository _machines;

    public MachinesController(IMachineRepository machines)
    {
        _machines = machines;
    }

    [HttpGet]
    public ActionResult<List<Machine>> GetAll()
    {
        return _machines.GetAll();
    }

    [HttpGet("{id}")]
    public ActionResult<Machine> GetById(string id)
    {
        var m = _machines.GetById(id);
        return m is null ? NotFound() : Ok(m);
    }

    [HttpPost]
    public IActionResult Upsert([FromBody] Machine machine)
    {
        if (string.IsNullOrWhiteSpace(machine.Id))
            return BadRequest("Machine.Id is required.");

        if (machine.LastSeen == default)
            machine.LastSeen = DateTime.UtcNow;

        _machines.Upsert(machine);
        return Ok();
    }

    [HttpPatch("{id}/lastSeen")]
    public IActionResult TouchLastSeen(string id)
    {
        _machines.UpdateLastSeen(id, DateTime.UtcNow);
        return Ok();
    }

    public sealed class UpdateStatusRequest
    {
        public MachineStatus Status { get; set; }
    }

    [HttpPatch("{id}/status")]
    public IActionResult UpdateStatus(string id, [FromBody] UpdateStatusRequest req)
    {
        _machines.UpdateStatus(id, req.Status);
        return Ok();
    }
}

