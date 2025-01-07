using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using UserAuthenticationJWT.Model;
using UserAuthenticationJWT.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
var connectionString = builder.Configuration.GetConnectionString("connectionDB");
builder.Services.AddSingleton(connectionString);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddOpenApiDocument();

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

//Endpoints

app.MapGet("/users", async (IUserRepository _userRepository) => {
    try
    {
        return Results.Ok(await _userRepository.GetAll());
    }
    catch(FileNotFoundException)
    {
        return Results.Problem("Server error", statusCode: 500);
    }
});

app.MapGet("/users/{userId}", async (IUserRepository _userRepository, int userId) => {
    try
    {
        User userFound = await _userRepository.GetById(userId);
        if(userFound == null) return Results.NotFound("User not found");

        return Results.Ok(userFound);
    }
    catch(FileNotFoundException)
    {
        return Results.Problem("Server error", statusCode: 500); 
    }
});

app.MapPost("/users/create", async (IUserRepository _userRepository, User newUser) => {
    try
    {
        await _userRepository.Create(newUser);
        return Results.NoContent();
    }
    catch(FileNotFoundException)
    {
        return Results.Problem("Server error", statusCode: 500);
    }
});

app.MapPut("/users/update/{userId}", async (IUserRepository _userRepository, int userId, User userToUpdate) => {
    try
    {
        if(await _userRepository.GetById(userId) == null) return Results.NotFound($"Not exists user with ID:{int.MinValue}");

        await _userRepository.Update(userToUpdate);
        return Results.Ok("User updated successfully");
    }
    catch(FileNotFoundException)
    {
        return Results.Problem("Server error", statusCode: 500);
    }
});

app.MapDelete("users/delete/{userId}", async (IUserRepository _userRepository, int userId) => {
    try
    {
        if(await _userRepository.GetById(userId) == null) return Results.NotFound($"Not exists user with ID:{int.MinValue}");

        await _userRepository.Delete(userId);
        return Results.Ok("User deleted successfully");
    }
    catch(FileNotFoundException)
    {
        return Results.Problem("Server error", statusCode: 500);
    }
});

app.Run();
