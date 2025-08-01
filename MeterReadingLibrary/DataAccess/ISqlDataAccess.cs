namespace MeterReadingLibrary.DataAccess;

public interface ISqlDataAccess
{
    Task<List<T>> LoadDataWithoutParameters<T>(string storedProcedure, string connectionStringName);
    Task<List<T>> LoadData<T, U>(string storedProcedure, U parameters, string connectionStringName);
    Task SaveData<T>(string storedProcedure, T parameters, string connectionStringName);
}