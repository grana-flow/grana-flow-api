using PlanWise.Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace tests.SignIn;

public class EntityTests
{
    [Fact]
    public void Validates_SignIn_With_All_Fields_Filled_In_Correctly()
    {
        var signin = new SignInVO { Email = "email@gmail.com", Password = "password" };

        var results = ValidateModel(signin);

        Assert.Empty(results);
    }

    [Fact]
    public void Validates_SignIn_With_Incorrect_Email_Address()
    {
        var signin = new SignInVO { Email = "emailgmail.com", Password = "password" };

        var results = ValidateModel(signin);

        Assert.NotEmpty(results);
        Assert.Contains(results, err => err.ErrorMessage == "Enter a valid email");
    }

    [Fact]
    public void Validates_SignIn_With_Empty_Password()
    {
        var signin = new SignInVO { Email = "emailgmail.com", Password = "" };

        var results = ValidateModel(signin);

        Assert.NotEmpty(results);
        Assert.Contains(results, err => err.ErrorMessage == "Password is required");
    }

    [Fact]
    public void Validates_SignIn_With_All_Fields_Empty()
    {
        var signin = new SignInVO { Email = "", Password = "" };

        var results = ValidateModel(signin);

        Assert.NotEmpty(results);
        Assert.Contains(results, err => err.ErrorMessage == "Password is required");
        Assert.Contains(results, err => err.ErrorMessage == "Email is required");
    }

    private List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, context, validationResults, true);
        return validationResults;
    }
}
