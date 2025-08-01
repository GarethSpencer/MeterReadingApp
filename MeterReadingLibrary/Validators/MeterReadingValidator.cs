using MeterReadingLibrary.DataAccess;
using MeterReadingLibrary.Models;

namespace MeterReadingLibrary.Validators;

public class MeterReadingValidator
{
    private readonly IStoredProcedureRunner _spRunner;

    public MeterReadingValidator(IStoredProcedureRunner spRunner)
    {
        _spRunner = spRunner;
    }

    public async Task<ValidationResult> ValidateMeterReading(MeterReadingInputModel input)
    {
        var result = new ValidationResult();

        var parsedaccountId = ValidateAccountId(input.AccountId, result.Errors);
        var parsedReadingDate = ValidateReadingDate(input.ReadingDate, result.Errors);
        var parsedReadingValue = ValidateReadingValue(input.ReadingValue, result.Errors);

        if (result.Errors.Count == 0)
        {
            result.ValidatedModel = new MeterReadingModel
            {
                AccountId = parsedaccountId,
                ReadingDate = parsedReadingDate,
                ReadingValue = parsedReadingValue
            };
        }
        else
        {
            return result;
        }

        await CheckValidReadingAgainstUser(result);

        result.IsValid = result.Errors.Count == 0;

        return result;
    }

    private static int ValidateAccountId(string accountId, List<string> errors)
    {
        if (!int.TryParse(accountId, out int parsedaccountId))
        {
            errors.Add("Invalid AccountId");
        }

        return parsedaccountId;
    }

    private static DateTime ValidateReadingDate(string readingDate, List<string> errors)
    {
        if (!DateTime.TryParse(readingDate, out DateTime parsedreadingDate))
        {
            errors.Add("Invalid ReadingDate");
        }

        return parsedreadingDate;
    }

    private static int ValidateReadingValue(string readingValue, List<string> errors)
    {
        if (!int.TryParse(readingValue, out int parsedreadingValue))
        {
            errors.Add("Invalid ReadingValue - not an integer");
        }
        else if (parsedreadingValue < 0)
        {
            errors.Add("Invalid ReadingValue - value is negative");
        }
        else if (parsedreadingValue > 99999)
        {
            errors.Add("Invalid ReadingValue - value is over 5 digits long");
        }

        return parsedreadingValue;
    }

    private async Task CheckValidReadingAgainstUser(ValidationResult result)
    {
        var matchingAccount = await _spRunner.GetAccountByAccountId(result.ValidatedModel!.AccountId);
        if (matchingAccount == null)
        {
            result.Errors.Add("Account does not exist");
            return;
        }

        var readingsForThisUser = await _spRunner.GetReadingsByAccountId(result.ValidatedModel!.AccountId);

        foreach (var reading in readingsForThisUser)
        {
            if (reading.AccountId == result.ValidatedModel!.AccountId &&
                reading.ReadingDate == result.ValidatedModel!.ReadingDate &&
                reading.ReadingValue == result.ValidatedModel!.ReadingValue)
            {
                result.Errors.Add("This reading already exists");
                return;
            }
            else if (reading.ReadingDate < result.ValidatedModel!.ReadingDate)
            {
                result.Errors.Add("This reading is older than an existing read");
                return;
            }
        }
    }
}