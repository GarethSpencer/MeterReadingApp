namespace MeterReadingLibrary.Models;

public class MeterReadingCsvRowModel
{
    public string? AccountId { get; set; }

    [CsvHelper.Configuration.Attributes.Name("MeterReadingDateTime")]
    public string? ReadingDate { get; set; }

    [CsvHelper.Configuration.Attributes.Name("MeterReadValue")]
    public string? ReadingValue { get; set; }
}