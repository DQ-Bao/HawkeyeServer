using System.Data;
using System.Text;
using HawkeyeServer.Api.Data;
using HawkeyeServer.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

public static class ConfigureServices
{
    public static IServiceCollection AddDataAccesses(
        this IServiceCollection services,
        string connectionString
    )
    {
        services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(connectionString));
        services.AddScoped<IUserDataAccess, UserDataAccess>();
        services.AddScoped<ITripDataAccess, TripDataAccess>();
        services.AddScoped<IPlaceDataAccess, PlaceDataAccess>();
        services.AddScoped<IActivityDataAccess, ActivityDataAccess>();
        services.AddSingleton<TripMemoryStore>();
        return services;
    }

    public static IServiceCollection AddJwtAuth(
        this IServiceCollection services,
        Action<JwtOptions> configureOptions
    )
    {
        var jwtOptions = new JwtOptions();
        configureOptions(jwtOptions);
        if (string.IsNullOrWhiteSpace(jwtOptions.Key))
            throw new InvalidOperationException("JwtOptions.Key must be set");
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
                opts.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Key)
                    ),
                }
            );
        services.AddAuthorization();
        services.Configure(configureOptions);
        services.AddTransient<JwtService>();
        return services;
    }
}
