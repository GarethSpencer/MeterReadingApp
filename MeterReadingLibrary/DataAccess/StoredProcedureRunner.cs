using MeterReadingLibrary.Models;

namespace MeterReadingLibrary.DataAccess;

public class StoredProcedureRunner : IStoredProcedureRunner
{
    private readonly ISqlDataAccess _sql;
    private readonly string _connectionStringName = "Default";

    public StoredProcedureRunner(ISqlDataAccess sql)
    {
        _sql = sql;
    }

    public Task<List<AccountModel>> GetAllAccounts()
    {
        return _sql.LoadDataWithoutParameters<AccountModel>(
            "[dbo].[spAccount_GetAll]",
            _connectionStringName);
    }

    public async Task<AccountModel?> GetAccountByAccountId(int accountId)
    {
        var results = await _sql.LoadData<AccountModel, dynamic>(
            "[dbo].[spAccount_GetByAccountId]",
            new { accountId },
            _connectionStringName);

        return results.FirstOrDefault();
    }

    public Task<List<MeterReadingModel>> GetReadingsByAccountId(int accountId)
    {
        return _sql.LoadData<MeterReadingModel, dynamic>(
            "[dbo].[spMeterReading_GetMeterReadingsForAccount]",
            new { accountId },
            _connectionStringName);
    }

    public Task<List<MeterReadingModel>> GetAllReadings()
    {
        return _sql.LoadDataWithoutParameters<MeterReadingModel>(
            "[dbo].[spMeterReading_GetAllMeterReadings]",
            _connectionStringName);
    }

    public Task AddReading(MeterReadingModel model)
    {
        return _sql.SaveData<dynamic>(
            "[dbo].[spMeterReading_AddMeterReading]",
            new { accountId = model.AccountId, readingDate = model.ReadingDate, readingValue = model.ReadingValue },
            _connectionStringName);
    }
}