using Bookify.Domain.UnitTests.Infrastructure;
using Bookify.Domain.Users;
using Bookify.Domain.Users.Events;

namespace Bookify.Domain.UnitTests.Users;

public class UserTests : BaseTests
{
    [Fact]
    public void Create_Should_SetPropertyValues()
    {
        // Act
        var user = User.Create(UserData.FirstName, UserData.lastName, UserData.Email);


        // Assert
        Assert.Equal(UserData.FirstName, user.FirstName);
        Assert.Equal(UserData.lastName, user.LastName);
        Assert.Equal(UserData.Email, user.Email);
    }

    [Fact]
    public void Create_Should_RaiseUserCreatedDomainEvent()
    {
        // Act
        var user = User.Create(UserData.FirstName, UserData.lastName, UserData.Email);

        // Assert
        var domainEvent = AssertDomainEventWasPublished<UserCreatedDomainEvent>(user);

        Assert.Equal(user.Id, domainEvent.UserId);
    }
}
