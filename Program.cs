using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UserAuthAndAuthenticationJWT.Models;
using UserAuthAndAuthenticationJWT.Repository.UserRepository;
using UserAuthAndAuthenticationJWT.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
var connectionString = builder.Configuration.GetConnectionString("connectionDB");
builder.Services.AddSingleton(connectionString);
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<JWTService>();

//Configuration Authentication JWT
builder.Services.AddAuthentication("Bearer").AddJwtBearer(opt => {
    opt.RequireHttpsMetadata = false;
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["JWTConfiguration:Issuer"],
        ValidAudience = builder.Configuration["JWTConfiguration:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWTConfiguration:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
    opt.SaveToken = true;
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

//Endpoints

app.MapGet("/login", async ([FromServices]JWTService _jwtService, [FromBody]Login user) => 
{
    try
    {
        var jwt = await _jwtService.GenerateJWT(user);
        if (jwt == null) return Results.Unauthorized();
        return Results.Ok(jwt);
    }
    catch(FileNotFoundException)
    {
        return Results.Problem("Server error", statusCode: 500);
    }
});

app.MapGet("/users", async (IUserRepository _userRepository) => {
    try
    {
        return Results.Ok(await _userRepository.GetAll());
    }
    catch(FileNotFoundException)
    {
        return Results.Problem("Server error", statusCode: 500);
    }
}).RequireAuthorization();

app.MapGet("/users/{userId}", async (IUserRepository _userRepository, int userId) => {
    try
    {
        User userFound = await _userRepository.GetBy(userId);
        if(userFound == null) return Results.NotFound("User not found");

        return Results.Ok(userFound);
    }
    catch(FileNotFoundException)
    {
        return Results.Problem("Server error", statusCode: 500); 
    }
}).RequireAuthorization();

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
}).RequireAuthorization();

app.MapPut("/users/update/{userId}", async (IUserRepository _userRepository, int userId, User userToUpdate) => {
    try
    {
        if(await _userRepository.GetBy(userId) == null) return Results.NotFound($"Not exists user with ID:{int.MinValue}");

        await _userRepository.Update(userToUpdate);
        return Results.Ok("User updated successfully");
    }
    catch(FileNotFoundException)
    {
        return Results.Problem("Server error", statusCode: 500);
    }
}).RequireAuthorization();

app.MapDelete("users/delete/{userId}", async (IUserRepository _userRepository, int userId) => {
    try
    {
        if(await _userRepository.GetBy(userId) == null) return Results.NotFound($"Not exists user with ID:{int.MinValue}");

        await _userRepository.Delete(userId);
        return Results.Ok("User deleted successfully");
    }
    catch(FileNotFoundException)
    {
        return Results.Problem("Server error", statusCode: 500);
    }
}).RequireAuthorization();

app.Run();
