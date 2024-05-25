using Backend.infrastructure.dataModels;
using Dapper;
using Npgsql;

namespace Backend.infrastructure.Repositories;

/**
 * Another class that is directly connected to the database.
 * This class focuses on the users-table
 */
public class UserRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public UserRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public int Create(string username, string email)
    {
        const string sql = $@"
INSERT INTO corsa.users (username, email)
VALUES (@username, @email)
RETURNING
    id as {nameof(User.id)}
";
        using var connection = _dataSource.OpenConnection();
        return connection.QueryFirst<int>(sql, new { username, email });
    }

    /**
     * Returns the user, with the given ID.
     */
    public User? GetById(int id)
    {
        const string sql = $@"
SELECT
    id as {nameof(User.id)},
    username as {nameof(User.username)},
    email as {nameof(User.email)}
FROM corsa.users
WHERE id = @id;
";
        using var connection = _dataSource.OpenConnection();
        return connection.QueryFirstOrDefault<User>(sql, new { id });
    }

    public IEnumerable<User> GetAll()
    {
        const string sql = $@"
SELECT
    id as {nameof(User.id)},
    username as {nameof(User.username)},
    email as {nameof(User.email)}
FROM corsa.users
";
        using (var conn = _dataSource.OpenConnection())
        {
            return conn.Query<User>(sql);
        }
    }
}