﻿namespace Bookify.Infrastructure.Authentication;

public sealed class AuthenticationOptions
{
    public string Audience { get; init; } = string.Empty;

    public string MetadataUrl { get; set; } = string.Empty;

    public bool RequireHttpsMetaData { get; init; }

    public string ValidIssuer { get; set; } = string.Empty;
}
