using Bookify.Application.Abstractions.Authentication;
using Bookify.Application.Abstractions.Messaging;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Users;

namespace Bookify.Application.Users.Commands;

internal sealed class LoginUserComandHandler(IJwtService jwtService) : ICommandHandler<LoginUserCommand, AccessTokenResponse>
{
    public async Task<Result<AccessTokenResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var result = await jwtService.GetAccessTokenAsync(request.Email, request.Password, cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<AccessTokenResponse>(UserErrors.InvalidCredentials);
        }

        return Result.Success(new AccessTokenResponse(result.Value));
    }
}

public sealed record LoginUserCommand(string Email, string Password) : ICommand<AccessTokenResponse>;

public sealed record AccessTokenResponse(string AccessToken);