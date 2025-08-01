namespace MeterReadingLibrary.Models;

internal class AccountModel
{
    public int AccountId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}