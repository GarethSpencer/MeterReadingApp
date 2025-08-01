using MeterReadingLibrary.Models;

namespace MeterReadingLibrary.DataAccess;

public interface IStoredProcedureRunner
{
    Task AddReading(MeterReadingModel model);
    Task<AccountModel?> GetAccountByAccountId(int accountId);
    Task<List<AccountModel>> GetAllAccounts();
    Task<List<MeterReadingModel>> GetAllReadings();
    Task<List<MeterReadingModel>> GetReadingsByAccountId(int accountId);
}