using Microsoft.AspNetCore.Identity;
using Moq;
using PlanWise.Domain.Contracts;
using PlanWise.Domain.Entities;
using PlanWise.Domain.Interfaces;

namespace tests.SignIn;

public class EndPointsTests
{
    private readonly Mock<IManageAccountRepository> _mockRepo;
    private const string _passwordIncorrect = "incorrectPassword";

    public EndPointsTests()
    {
        _mockRepo = new Mock<IManageAccountRepository>();
    }

    [Fact]
    public async Task Validates_User_With_Correct_Password()
    {
        var createUser = new CreateUser
        {
            Username = "leoo",
            Email = "email_test@gmail.com",
            Password = "password",
            ConfirmPassword = "password"
        };
        var user = User.CreateUser(createUser);

        _mockRepo
            .Setup(repo => repo.CreateAccount(user, createUser.Password))
            .ReturnsAsync(IdentityResult.Success);

        _mockRepo.Setup(repo => repo.CheckPassword(user, createUser.Password)).ReturnsAsync(true);

        var resultCreated = await _mockRepo.Object.CreateAccount(user, createUser.Password);
        var resultCheckPassword = await _mockRepo.Object.CheckPassword(user, createUser.Password);

        Assert.True(resultCreated.Succeeded);
        Assert.True(resultCheckPassword);
    }

    [Fact]
    public async Task Validates_User_With_Incorrect_Password()
    {
        var createUser = new CreateUser
        {
            Username = "leoo",
            Email = "email_test@gmail.com",
            Password = "password",
            ConfirmPassword = "password"
        };
        var user = User.CreateUser(createUser);

        _mockRepo
            .Setup(repo => repo.CreateAccount(user, createUser.Password))
            .ReturnsAsync(IdentityResult.Success);

        _mockRepo.Setup(repo => repo.CheckPassword(user, _passwordIncorrect)).ReturnsAsync(false);

        var resultCreated = await _mockRepo.Object.CreateAccount(user, createUser.Password);
        var resultCheckPassword = await _mockRepo.Object.CheckPassword(user, _passwordIncorrect);

        Assert.True(resultCreated.Succeeded);
        Assert.False(resultCheckPassword);
    }
}
