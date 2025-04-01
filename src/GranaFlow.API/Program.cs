using Microsoft.AspNetCore.Identity;
using GranaFlow.Domain.Entities;
using GranaFlow.Infra.Data.Context;
using GranaFlow.Infra.Ioc.Configs.Swagger;
using GranaFlow.Infra.Ioc.DependencyInjection;
using RabbitMQServer.services;

var builder = WebApplication.CreateBuilder(args);
var structureDependencies = new DependencyInjectionIdentity(
    builder.Services,
    builder.Configuration
);

structureDependencies.AddDbContext();

structureDependencies.AddScopedAndDependencies();
builder.Services.AddHostedService<RabbitMQConfirmEmailConsumer>();
builder.Services.AddHostedService<RabbitMQ2FAConsumer>();
builder.Services.AddHostedService<RabbitMQForgetPasswordConsumer>();

builder
    .Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContextIdentity>()
    .AddDefaultTokenProviders();

structureDependencies.ConfigurePasswordRules();

structureDependencies.AddConfigAuthentication();

builder.Services.AddAuthorization();

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerConfiguration();

structureDependencies.AddApiVersioning();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwaggerConfiguration();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
