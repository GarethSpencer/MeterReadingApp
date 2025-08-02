using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MeterReadingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthenticationController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(IConfiguration config, ILogger<AuthenticationController> logger)
    {
        _config = config;
        _logger = logger;
    }

    public record AuthenticationData(string? UserName, string? Password);
    public record UserData(int Id, string UserName);

    // POST api/Authentication/token
    [HttpPost("token")]
    [AllowAnonymous]
    public ActionResult<string> Authenticate([FromBody] AuthenticationData data)
    {
        try
        {
            var user = ValidateCredentials(data);

            if (user is null)
            {
                _logger.LogWarning($"Unauthorised user [{data.UserName}] has failed authentication");
                return Unauthorized();
            }

            var token = GenerateToken(user);

            _logger.LogInformation("Authentication successful! A token has been returned.");
            return Ok(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The POST call to Authentication/token has failed.");
            return BadRequest();
        }
    }

    private string GenerateToken(UserData user)
    {
        var secretKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(_config.GetValue<string>("Authentication:SecretKey")!));

        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
        ];

        var token = new JwtSecurityToken(_config.GetValue<string>("Authentication:Issuer"),
            _config.GetValue<string>("Authentication:Audience"),
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            signingCredentials
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserData? ValidateCredentials(AuthenticationData data)
    {
        // NON PRODUCTION VALIDATIONS: Replace this with a call to an auth system (e.g. Auth0)
        if (CompareValues(data.UserName, "ensek") &&
            CompareValues(data.Password, "Test1!"))
        {
            return new UserData(1, data.UserName!);
        }

        return null;
    }

    private static bool CompareValues(string? actual, string expected)
    {
        if (actual is not null && actual.Equals(expected))
        {
            return true;
        }

        return false;
    }
}
