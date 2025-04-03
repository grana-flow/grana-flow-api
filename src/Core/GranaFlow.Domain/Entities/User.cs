using GranaFlow.Domain.Contracts;
using Microsoft.AspNetCore.Identity;

namespace GranaFlow.Domain.Entities;

public sealed class User : IdentityUser
{
    public static User CreateUser(CreateUser model)
    {
        return new User { UserName = model.Username, Email = model.Email };
    }
}
