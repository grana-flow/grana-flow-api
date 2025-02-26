using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Moq;
using PlanWise.Application.DTOs;
using PlanWise.Application.Mappings;
using PlanWise.Domain.Entities;
using PlanWise.Domain.Interfaces;

namespace tests.SignIn;

public class EndPointsTests
{
    private readonly Mock<IManageAccountRepository> _mockRepo;
    private readonly IMapper _mapper;
    private const string _passwordIncorrect = "incorrectPassword";

    public EndPointsTests()
    {
        _mockRepo = new Mock<IManageAccountRepository>();
        var config = DomainToMappingUser.RegisterMaps();
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Validates_User_With_Correct_Password()
    {
        var userVO = new UserVO
        {
            Username = "leoo",
            Email = "email_test@gmail.com",
            Password = "password",
            ConfirmPassword = "password"
        };
        var user = _mapper.Map<User>(userVO);

        _mockRepo
            .Setup(repo => repo.CreateAccount(user, userVO.Password))
            .ReturnsAsync(IdentityResult.Success);

        _mockRepo.Setup(repo => repo.CheckPassword(user, userVO.Password)).ReturnsAsync(true);

        var resultCreated = await _mockRepo.Object.CreateAccount(user, userVO.Password);
        var resultCheckPassword = await _mockRepo.Object.CheckPassword(user, userVO.Password);

        Assert.True(resultCreated.Succeeded);
        Assert.True(resultCheckPassword);
    }

    [Fact]
    public async Task Validates_User_With_Incorrect_Password()
    {
        var userVO = new UserVO
        {
            Username = "leoo",
            Email = "email_test@gmail.com",
            Password = "password",
            ConfirmPassword = "password"
        };
        var user = _mapper.Map<User>(userVO);

        _mockRepo
            .Setup(repo => repo.CreateAccount(user, userVO.Password))
            .ReturnsAsync(IdentityResult.Success);

        _mockRepo.Setup(repo => repo.CheckPassword(user, _passwordIncorrect)).ReturnsAsync(false);

        var resultCreated = await _mockRepo.Object.CreateAccount(user, userVO.Password);
        var resultCheckPassword = await _mockRepo.Object.CheckPassword(user, _passwordIncorrect);

        Assert.True(resultCreated.Succeeded);
        Assert.False(resultCheckPassword);
    }
}
