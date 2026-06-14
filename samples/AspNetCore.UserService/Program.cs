using AppFactory.Framework.Api.AspNetCore.Extensions;
using AppFactory.Framework.Api.AspNetCore.Middleware;
using AppFactory.Framework.Domain.Repositories;
using AppFactory.Framework.Logging.MicrosoftExtensions;
using AspNetCore.UserService.Application.Commands;
using AspNetCore.UserService.Application.DTOs;
using AspNetCore.UserService.Application.Queries;
using AspNetCore.UserService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMicrosoftExtensionsLogging();

builder.Services.AddAppFactoryApi(typeof(Program).Assembly);

// Register in-memory repository for demo purposes
builder.Services.AddSingleton<IRepository<AspNetCore.UserService.Domain.User>, InMemoryUserRepository>();

builder.Services.AddOpenApi();

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapCommand<CreateUserCommand, UserDto>("/api/users")
   .WithName("CreateUser")
   .WithSummary("Create a new user")
   .WithDescription("Creates a new user with the specified email and name")
   .WithTags("Users");

app.MapQuery<GetUserByIdQuery, UserDto>("/api/users/{userId}")
   .WithName("GetUser")
   .WithSummary("Get user by ID")
   .WithDescription("Retrieves a user by their unique identifier")
   .WithTags("Users");

app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok(new { service = "User Service", version = "1.0.0", status = "running" }));

app.Run();
