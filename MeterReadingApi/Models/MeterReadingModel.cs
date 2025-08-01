namespace MeterReadingLibrary.Models;

public class MeterReadingModel
{
    public int MeterReadingId { get; set; }
    public int AccountId { get; set; }

    public DateTime ReadingDate { get; set; }

    public int ReadingValue { get; set; }
}