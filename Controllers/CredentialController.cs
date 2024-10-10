using Microsoft.AspNetCore.Mvc;
using CredentialManagerPOC.Services;

namespace CredentialManagerPOC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CredentialController : ControllerBase
    {
        private readonly ICredentialService _credentialService;

        public CredentialController(ICredentialService credentialService)
        {
            _credentialService = credentialService;
        }

        [HttpGet("get-connection-string")]
        public IActionResult GetConnectionString(string targetName)
        {
            try
            {
                var connectionString = _credentialService.GetConnectionString(targetName);
                if (!string.IsNullOrEmpty(connectionString))
                {
                    return Ok(new { ConnectionString = connectionString });
                }
                else
                {
                    return NotFound("Connection string not found.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
