using AppFactory.Framework.Api.AspNetCore.Extensions;
using AppFactory.Framework.Api.AspNetCore.Middleware;
using AppFactory.Framework.Api.Parsing.Configurations;
using AppFactory.Framework.Logging.MicrosoftExtensions;
using AspNetCore.UserService.Domain.Users;
using AspNetCore.UserService.Features.Users.CreateUser;
using AspNetCore.UserService.Features.Users.GetUserById;
using AspNetCore.UserService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Services.AddMicrosoftExtensionsLogging();

// API Framework
builder.Services.AddAppFactoryApi(typeof(Program).Assembly);

// Register parse model maps for request mapping
builder.Services.AddSingleton<IParseModelMap, CreateUserRequestMap>();
builder.Services.AddSingleton<IParseModelMap, GetUserByIdQueryMap>();

// Infrastructure - Repositories
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

// API Documentation
builder.Services.AddOpenApi();

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Feature Endpoints
app.MapCreateUserEndpoint();
app.MapGetUserByIdEndpoint();

// Health & Info
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok(new 
{ 
    service = "User Service", 
    version = "1.0.0", 
    status = "running",
    architecture = "Vertical Slices + DDD + Clean Architecture"
}));

app.Run();
