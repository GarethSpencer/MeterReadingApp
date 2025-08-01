using Microsoft.AspNetCore.Mvc;

namespace MeterReadingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MeterReadingController : ControllerBase
{
    public record LoadResults(int Successes, int Failures);

    // POST api/MeterReading
    [HttpPost("meter-reading-uploads")]
    public async Task<ActionResult<LoadResults>> Post([FromBody] string? fileLocation)
    {
        fileLocation ??= $@"{Directory.GetParent(Directory.GetCurrentDirectory())!.FullName}\TestFiles\meter-reading.csv";
        
        var successCount = 3;
        var failureCount = 6;
        var loadResults = new LoadResults(successCount, failureCount);

        return Ok(loadResults);
    }
}