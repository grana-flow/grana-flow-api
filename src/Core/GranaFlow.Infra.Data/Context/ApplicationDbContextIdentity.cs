using GranaFlow.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GranaFlow.Infra.Data.Context;

public class ApplicationDbContextIdentity(DbContextOptions<ApplicationDbContextIdentity> options)
    : IdentityDbContext<User>(options) { }
