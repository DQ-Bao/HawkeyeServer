using HawkeyeServer.Api.Models;

namespace HawkeyeServer.Api.Data;

public static class UserCommand
{
    public const string GetAll = "select * from users order by created_at desc;";
    public const string GetByEmail = "select * from users where email = @Email;";
    public const string Add =
        @"insert into users(email, name, picture)
        values (@Email, @Name, @Picture)
        returning id;";
}

public interface IUserDataAccess
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByEmail(string email);
    Task<User> AddAsync(User user);
}
