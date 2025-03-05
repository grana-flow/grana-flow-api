using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlanWise.Domain.Entities;

namespace PlanWise.Infra.Data.Context;

public class ApplicationDbContextIdentity(DbContextOptions<ApplicationDbContextIdentity> options)
    : IdentityDbContext<User>(options) { }
