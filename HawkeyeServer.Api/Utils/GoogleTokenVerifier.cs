using Google.Apis.Auth;

namespace HawkeyeServer.Api.Utils;

public class GoogleTokenVerifier(IConfiguration configuration)
{
    private readonly string _clientId = configuration["Google:ClientId"]!;

    public async Task<GoogleJsonWebSignature.Payload> VerifyAsync(string idToken)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _clientId },
        };
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        return payload;
    }
}
