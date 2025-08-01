using MeterReadingLibrary.DataAccess;
using Microsoft.AspNetCore.Mvc;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using MeterReadingLibrary.Models;
using MeterReadingLibrary.Validators;
using MeterReadingApi.Helpers;

namespace MeterReadingApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MeterReadingController : ControllerBase
{
    private readonly IStoredProcedureRunner _spRunner;
    private readonly ILogger<MeterReadingController> _logger;

    public MeterReadingController(IStoredProcedureRunner spRunner, ILogger<MeterReadingController> logger)
    {
        _spRunner = spRunner;
        _logger = logger;
    }

    // POST api/MeterReading/meter-reading-uploads
    [HttpPost("meter-reading-uploads")]
    public async Task<ActionResult<LoadResults>> Post([FromBody] string? fileLocation)
    {
        fileLocation ??= $@"{Directory.GetCurrentDirectory()}\TestFiles\meter-reading.csv";

        var results = new LoadResults();
        using var reader = new StreamReader(fileLocation);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            IgnoreBlankLines = true
        });

        var csvReadings = csv.GetRecords<MeterReadingCsvRow>().ToList();

        var inputRows = csvReadings.Select(x => new MeterReadingInputModel
        {
            AccountId = x.AccountId,
            ReadingDate = x.ReadingDate,
            ReadingValue = x.ReadingValue
        }).ToList();

        var validator = new MeterReadingValidator(_spRunner);

        foreach (var reading in inputRows)
        {
            var validation = await validator.ValidateMeterReading(reading);
            if (validation.IsValid)
            {
                await _spRunner.AddReading(validation.ValidatedModel!);
                results.Successes++;
            }
            else
            {
                results.Failures++;
            }
        }

        return Ok(results);
    }
}