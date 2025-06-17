using Bookify.Api.FunctionalTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace Bookify.Api.FunctionalTests.Users;

public class RegisterUserTests(FunctionalTestWebAppFactory factory) : BaseFunctionalTest(factory)
{
    // Add your test methods here
    // Example:
    [Fact]
    public async Task RegisterUser_ShouldReturnOk_WhenDataIsValid()
    {
        // Arrange
        var request = UserData.RegisterTestUserRequest;

        // Act
        var response = await _client.PostAsJsonAsync("api/v1/users/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
