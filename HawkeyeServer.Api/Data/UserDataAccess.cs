using System.Data;
using Dapper;
using HawkeyeServer.Api.Models;

namespace HawkeyeServer.Api.Data;

public class UserDataAccess(IDbConnection db) : IUserDataAccess
{
    public async Task<IEnumerable<User>> GetAllAsync() =>
        await db.QueryAsync<User>(UserCommand.GetAll);

    public async Task<User?> GetByEmail(string email) =>
        await db.QueryFirstOrDefaultAsync<User>(UserCommand.GetByEmail, new { Email = email });

    public async Task<User> AddAsync(User user) =>
        user with
        {
            Id = await db.ExecuteScalarAsync<long>(UserCommand.Add, user),
        };
}
