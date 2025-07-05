using System.ComponentModel.DataAnnotations;
using Google.Apis.Auth;
using HawkeyeServer.Api.Data;
using HawkeyeServer.Api.Models;
using HawkeyeServer.Api.Services;
using HawkeyeServer.Api.Utils;
using Microsoft.AspNetCore.Mvc;

namespace HawkeyeServer.Api.Endpoints;

public record AuthRequest([Required] string IdToken);

public record AuthResponse(string Token);

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/auth/google",
                async (
                    [FromBody] AuthRequest request,
                    GoogleTokenVerifier verifier,
                    JwtService jwt,
                    IUserDataAccess users
                ) =>
                {
                    GoogleJsonWebSignature.Payload payload;
                    try
                    {
                        payload = await verifier.VerifyAsync(request.IdToken);
                    }
                    catch (InvalidJwtException)
                    {
                        return Results.BadRequest("Invalid token");
                    }
                    var user =
                        await users.GetByEmail(payload.Email)
                        ?? await users.AddAsync(
                            new User
                            {
                                Email = payload.Email,
                                Name = payload.Name,
                                Picture = payload.Picture,
                            }
                        );
                    var token = jwt.GenerateToken(user);
                    return Results.Ok(new AuthResponse(token));
                }
            )
            .AddEndpointFilter<ValidationFilter<AuthRequest>>();
        return app;
    }
}
