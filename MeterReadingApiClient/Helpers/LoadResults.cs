namespace MeterReadingApiClient.Helpers;

public class LoadResults()
{
    public int Successes { get; set; }
    public int Failures { get; set; }
    public List<string> ErrorMessages { get; set; } = [];
}