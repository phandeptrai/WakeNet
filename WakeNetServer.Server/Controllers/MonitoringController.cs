using Microsoft.AspNetCore.Mvc;
using WakeNetServer.Server.Models;
using WakeNetServer.Server.Repository.Interfaces;

namespace WakeNetServer.Server.Controllers;

// API lưu/lấy thông tin giám sát phần cứng (SystemInfo).
[ApiController]
[Route("api/[controller]")]
public sealed class MonitoringController : ControllerBase
{
    private readonly IMonitoringRepository _monitoring;

    public MonitoringController(IMonitoringRepository monitoring)
    {
        _monitoring = monitoring;
    }

    [HttpPost("systemInfo")]
    public IActionResult SaveSystemInfo([FromBody] SystemInfo info)
    {
        if (string.IsNullOrWhiteSpace(info.MachineId))
            return BadRequest("SystemInfo.MachineId is required.");

        _monitoring.SaveLatest(info);
        return Ok();
    }

    [HttpGet("latest/{machineId}")]
    public ActionResult<SystemInfo> GetLatest(string machineId)
    {
        var info = _monitoring.GetLatest(machineId);
        return info is null ? NotFound() : Ok(info);
    }
}

