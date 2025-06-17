using Bookify.Application.Users.Commands;

namespace Bookify.Api.FunctionalTests.Users;

public class UserData
{
    public static RegisterUserCommand RegisterTestUserRequest = new("test@test.com", "test", "test", "12345");
}
