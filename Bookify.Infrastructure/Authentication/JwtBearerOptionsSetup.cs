using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Bookify.Infrastructure.Authentication;

internal class JwtBearerOptionsSetup(IOptions<AuthenticationOptions> authenticationOptions) : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly AuthenticationOptions _authenticationOptions = authenticationOptions.Value;

    public void Configure(string? name, JwtBearerOptions options)
    {
        Configure(options);
    }

    public void Configure(JwtBearerOptions options)
    {
        options.Audience = _authenticationOptions.Audience;
        options.MetadataAddress = _authenticationOptions.MetadataUrl;
        options.RequireHttpsMetadata = _authenticationOptions.RequireHttpsMetaData;
        options.TokenValidationParameters.ValidIssuer = _authenticationOptions.ValidIssuer;
    }
}
