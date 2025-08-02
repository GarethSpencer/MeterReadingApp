using CsvHelper;
using CsvHelper.Configuration;
using MeterReadingApi.Helpers;
using MeterReadingLibrary.DataAccess;
using MeterReadingLibrary.Models;
using MeterReadingLibrary.Validators;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

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
    public async Task<ActionResult<LoadResults>> Post([FromBody] string fileLocation)
    {
        try
        {
            List<MeterReadingInputModel> inputRows = GetInputRows(fileLocation);
            var validator = new MeterReadingValidator(_spRunner);
            var loadResults = new LoadResults();
            int rowNumber = 2;

            foreach (var reading in inputRows)
            {
                var validation = await validator.ValidateMeterReading(reading);
                if (validation.IsValid)
                {
                    await _spRunner.AddReading(validation.ValidatedModel!);
                    loadResults.Successes++;
                }
                else
                {
                    LogValidationErrors(rowNumber, validation.Errors, loadResults, reading.AccountId ?? string.Empty);
                    loadResults.Failures++;
                }
                rowNumber++;
            }

            return Ok(loadResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The POST call to MeterReading/meter-reading-uploads has failed.");
            return BadRequest();
        }
    }

    private List<MeterReadingInputModel> GetInputRows(string fileLocation)
    {
        using var reader = new StreamReader(fileLocation);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ","
        });

        var csvReadings = csv.GetRecords<MeterReadingCsvRow>().ToList();

        _logger.LogInformation($"The csv File at {fileLocation} has been successfully parsed.");

        return csvReadings.Select(x => new MeterReadingInputModel
        {
            AccountId = x.AccountId,
            ReadingDate = x.ReadingDate,
            ReadingValue = x.ReadingValue
        }).ToList();
    }

    private void LogValidationErrors(int rowNumber, List<string> errors, LoadResults loadResults, string accountId)
    {
        foreach (var error in errors)
        {
            var message = $"Error loading row [{rowNumber}] of the file for AccountId [{accountId}]: {error}";
            loadResults.ErrorMessages.Add(message);
            _logger.LogWarning(message);
        }
    }
}