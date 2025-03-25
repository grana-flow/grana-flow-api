using Microsoft.AspNetCore.Identity;
using GranaFlow.Domain.Contracts;

namespace GranaFlow.Domain.Entities;

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
