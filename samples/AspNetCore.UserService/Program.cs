using AppFactory.Framework.Api.AspNetCore.Extensions;
using AppFactory.Framework.Api.AspNetCore.Middleware;
using AspNetCore.UserService.Application.Commands;
using AspNetCore.UserService.Application.DTOs;
using AspNetCore.UserService.Application.Queries;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMicrosoftExtensionsLogging();

builder.Services.AddAppFactoryApi(typeof(Program).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "User Service API", Version = "v1" });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapCommand<CreateUserCommand, UserDto>("/api/users")
   .WithName("CreateUser")
   .WithOpenApi()
   .WithSummary("Create a new user")
   .WithDescription("Creates a new user with the specified email and name");

app.MapQuery<GetUserByIdQuery, UserDto>("/api/users/{userId}")
   .WithName("GetUser")
   .WithOpenApi()
   .WithSummary("Get user by ID")
   .WithDescription("Retrieves a user by their unique identifier");

app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok(new { service = "User Service", version = "1.0.0", status = "running" }));

app.Run();
