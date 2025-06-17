using Bookify.Api.FunctionalTests.Users;
using Bookify.Application.Users.Commands;
using System.Net.Http.Json;

namespace Bookify.Api.FunctionalTests.Infrastructure;

public abstract class BaseFunctionalTest(FunctionalTestWebAppFactory factory) : IClassFixture<FunctionalTestWebAppFactory>
{
    protected readonly HttpClient _client = factory.CreateClient();

    protected async Task<string> GetAccessToken()
    {
        HttpResponseMessage loginResponse = await _client.PostAsJsonAsync(
           "api/v1/users/login",
           new LoginUserCommand(
               UserData.RegisterTestUserRequest.Email,
               UserData.RegisterTestUserRequest.Password));

        var accessTokenResponse = await loginResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();

        return accessTokenResponse!.AccessToken;
    }
}