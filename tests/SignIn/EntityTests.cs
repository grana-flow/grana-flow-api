using GranaFlow.Domain.Contracts;
using System.ComponentModel.DataAnnotations;

namespace tests.SignIn;

public class EntityTests
{
    [Fact]
    public void Validates_User_With_All_Fields_Filled_In_Correctly()
    {
        var createUser = new CreateUser
        {
            Username = "userTest",
            Email = "user_test@gmail.com",
            Password = "pass@123",
            ConfirmPassword = "pass@123"
        };

        var results = ValidateModel(createUser);

        Assert.Empty(results);
    }

    [Fact]
    public void Validates_User_With_Incorrect_Email_Address()
    {
        var createUser = new CreateUser
        {
            Username = "userTest",
            Email = "user_testgmail.com",
            Password = "pass@123",
            ConfirmPassword = "pass@123"
        };

        var results = ValidateModel(createUser);

        Assert.NotEmpty(results);
        Assert.Contains(results, err => err.ErrorMessage == "Enter a valid email");
    }

    [Fact]
    public void Validates_User_With_Incompatible_Password_Confirm()
    {
        var createUser = new CreateUser
        {
            Username = "userTest",
            Email = "user_test@gmail.com",
            Password = "pass@123",
            ConfirmPassword = "pass123"
        };

        var results = ValidateModel(createUser);

        Assert.NotEmpty(results);
        Assert.Contains(
            results,
            err => err.ErrorMessage == "The password and confirmation password do not match"
        );
    }

    [Fact]
    public void Validates_User_With_Empty_Fields()
    {
        var createUser = new CreateUser
        {
            Username = "",
            Email = "",
            Password = "",
            ConfirmPassword = ""
        };

        var results = ValidateModel(createUser);

        Assert.NotEmpty(results);
        Assert.Contains(results, err => err.ErrorMessage == "Email is required");
        Assert.Contains(results, err => err.ErrorMessage == "Username is required");
        Assert.Contains(results, err => err.ErrorMessage == "Password is required");
        Assert.Contains(results, err => err.ErrorMessage == "ConfirmPassword is required");
    }

    private List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, context, validationResults, true);
        return validationResults;
    }
}
