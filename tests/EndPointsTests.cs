using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using PlanWise.Domain.Contracts;
using PlanWise.Domain.Entities;
using PlanWise.Domain.Interfaces;

namespace tests;

public class EndPointsTests
{
    private readonly Mock<IManageAccountRepository> _mockRepo;
    private const string _passwordIncorrect = "incorrectPassword";
    private const string _nonExistentEmail = "email_non-existent@gmail.com";
    private const string _newPassword = "newPassword";
    private const string _forgetPasswordToken = "forget-password_token";

    public EndPointsTests()
    {
        _mockRepo = new Mock<IManageAccountRepository>();
    }

    [Fact]
    public async Task Register_User_Successfully()
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

        var result = await _mockRepo.Object.CreateAccount(user, createUser.Password);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Search_User_By_Existing_Email()
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

        _mockRepo.Setup(repo => repo.FindByEmail(user.Email!)).ReturnsAsync(user);

        var resultCreated = await _mockRepo.Object.CreateAccount(user, createUser.Password);
        var resultUser = await _mockRepo.Object.FindByEmail(createUser.Email);

        Assert.True(resultCreated.Succeeded);
        Assert.NotNull(resultUser);
        Assert.Equal(resultUser.Email, createUser.Email);
        Assert.Equal(resultUser.UserName, createUser.Username);
    }

    [Fact]
    public async Task Search_User_By_Non_Existent_Email()
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

        _mockRepo.Setup(repo => repo.FindByEmail(_nonExistentEmail)).ReturnsAsync((User?)null);

        var resultCreated = await _mockRepo.Object.CreateAccount(user, createUser.Password);
        var resultUser = await _mockRepo.Object.FindByEmail(_nonExistentEmail);

        Assert.True(resultCreated.Succeeded);
        Assert.Null(resultUser);
    }

    [Fact]
    public async Task Validate_Password_Change_Successfully()
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

        _mockRepo
            .Setup(repo => repo.GenerateForgetPasswordToken(user))
            .ReturnsAsync(_forgetPasswordToken);

        _mockRepo
            .Setup(repo => repo.ValidateResetPassword(user, _forgetPasswordToken, _newPassword))
            .ReturnsAsync(IdentityResult.Success);

        _mockRepo.Setup(repo => repo.CheckPassword(user, _newPassword)).ReturnsAsync(true);

        var resultCreated = await _mockRepo.Object.CreateAccount(user, createUser.Password);
        var resultForgetPasswordToken = await _mockRepo.Object.GenerateForgetPasswordToken(user);
        var resultValidateResetPassword = await _mockRepo.Object.ValidateResetPassword(
            user,
            resultForgetPasswordToken,
            _newPassword
        );
        var resultCheckPassword = await _mockRepo.Object.CheckPassword(user, _newPassword);

        Assert.True(resultCreated.Succeeded);
        Assert.NotNull(resultForgetPasswordToken);
        Assert.True(resultValidateResetPassword.Succeeded);
        Assert.True(resultCheckPassword);
    }

    [Fact]
    public async Task Check_Password_After_Forgetting_It_With_Incorrect_New_Password()
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

        _mockRepo
            .Setup(repo => repo.GenerateForgetPasswordToken(user))
            .ReturnsAsync(_forgetPasswordToken);

        _mockRepo
            .Setup(repo => repo.ValidateResetPassword(user, _forgetPasswordToken, _newPassword))
            .ReturnsAsync(IdentityResult.Success);

        _mockRepo.Setup(repo => repo.CheckPassword(user, "any password")).ReturnsAsync(false);

        var resultCreated = await _mockRepo.Object.CreateAccount(user, createUser.Password);
        var resultForgetPasswordToken = await _mockRepo.Object.GenerateForgetPasswordToken(user);
        var resultValidateResetPassword = await _mockRepo.Object.ValidateResetPassword(
            user,
            resultForgetPasswordToken,
            _newPassword
        );
        var resultCheckPassword = await _mockRepo.Object.CheckPassword(user, "any password");

        Assert.True(resultCreated.Succeeded);
        Assert.NotNull(resultForgetPasswordToken);
        Assert.True(resultValidateResetPassword.Succeeded);
        Assert.False(resultCheckPassword);
    }
}
