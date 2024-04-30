using Npgsql;

namespace Backend.infrastructure;

public class DatabaseConnector
{
    private NpgsqlDataSource _dataSource;

    public DatabaseConnector(NpgsqlDataSource datasource)
    {
        _dataSource = datasource;
    }
}