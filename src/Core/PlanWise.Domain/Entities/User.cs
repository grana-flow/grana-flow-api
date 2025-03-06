using Microsoft.AspNetCore.Identity;
using PlanWise.Domain.Contracts;

namespace PlanWise.Domain.Entities;

public sealed class User : IdentityUser
{
    public static User CreateUser(CreateUser model)
    {
        return new User
        {
            UserName = model.Username,
            Email = model.Email
        };
    }
}
