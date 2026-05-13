using AIAgent.Core.Models;
using AIAgent.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public SettingsController(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    [HttpGet]
    public async Task<ActionResult<AISettings>> GetSettings()
    {
        var settings = await _settingsService.GetSettingsAsync();
        return Ok(settings);
    }

    [HttpPost]
    public async Task<IActionResult> SaveSettings([FromBody] AISettings settings)
    {
        await _settingsService.SaveSettingsAsync(settings);
        return Ok();
    }

    [HttpGet("active")]
    public async Task<ActionResult<AIProviderConfig?>> GetActiveProvider()
    {
        var provider = await _settingsService.GetActiveProviderAsync();
        return Ok(provider);
    }

    [HttpPost("active/{providerName}")]
    public async Task<IActionResult> SetActiveProvider(string providerName)
    {
        await _settingsService.SetActiveProviderAsync(providerName);
        return Ok();
    }

    [HttpPost("validate")]
    public async Task<ActionResult<bool>> ValidateProvider([FromBody] AIProviderConfig provider)
    {
        var isValid = await _settingsService.ValidateProviderAsync(provider);
        return Ok(isValid);
    }
}
