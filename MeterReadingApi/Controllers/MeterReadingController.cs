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

    /// <summary>
    ///     Takes a .csv file of user meter readings, validates and loads it to a database
    /// </summary>
    /// <remarks>
    ///     Send the file as form-data in the body of the request, with key 'file'.
    /// </remarks>
    /// <param name="file"></param>
    /// <returns>
    ///     Success and failure volumes, with a list of errors for invalid rows
    /// </returns>
    // POST api/MeterReading/meter-reading-uploads
    [HttpPost("meter-reading-uploads")]
    public async Task<ActionResult<LoadResults>> Post(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogError("The POST call to MeterReading/meter-reading-uploads did not contain a file.");
                return BadRequest("No file uploaded.");
            }

            List<MeterReadingInputModel> inputRows = GetInputRows(file);
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

    private static List<MeterReadingInputModel> GetInputRows(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ","
        });

        var csvReadings = csv.GetRecords<MeterReadingCsvRowModel>().ToList();

        return [.. csvReadings.Select(x => new MeterReadingInputModel
        {
            AccountId = x.AccountId,
            ReadingDate = x.ReadingDate,
            ReadingValue = x.ReadingValue
        })];
    }

    private void LogValidationErrors(int rowNumber, List<string> errors, LoadResults loadResults, string accountId)
    {
        foreach (var error in errors)
        {
            var message = $"Error loading row [{rowNumber}] for AccountId [{accountId}]: {error}";
            loadResults.ErrorMessages.Add(message);
            _logger.LogWarning(message);
        }
    }
}