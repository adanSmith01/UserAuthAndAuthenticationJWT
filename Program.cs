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

app.MapGet("/login", async ([FromServices]JWTService _jwtService, [FromBody]Login user, ILogger _logger) => 
{
    try
    {
        var jwt = await _jwtService.GenerateJWT(user);
        if (jwt == null) return Results.Unauthorized();
        return Results.Ok(jwt);
    }
    catch(FileNotFoundException ex)
    {
        _logger.LogInformation(ex.ToString());
        return Results.Problem("Server error", statusCode: 500);
    }
    catch(Exception ex)
    {
        _logger.LogWarning(ex.ToString());
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/users", async (IUserRepository _userRepository, ILogger _logger) => {
    try
    {
        return Results.Ok(await _userRepository.GetAll());
    }
    catch(FileNotFoundException ex)
    {
        _logger.LogInformation(ex.ToString());
        return Results.Problem("Server error", statusCode: 500);
    }
    catch(Exception ex)
    {
        _logger.LogWarning(ex.ToString());
        return Results.Problem(ex.Message);
    }
}).RequireAuthorization();

app.MapGet("/users/{userId}", async (IUserRepository _userRepository, int? userId, ILogger _logger) => {
    try
    {
        User userFound = await _userRepository.GetBy(userId ?? 0);
        if(userFound == null) return Results.NotFound("User not found");

        return Results.Ok(userFound);
    }
    catch(FileNotFoundException ex)
    {
        _logger.LogInformation(ex.ToString());
        return Results.Problem("Server error", statusCode: 500);
    }
    catch(Exception ex)
    {
        _logger.LogWarning(ex.ToString());
        return Results.Problem(ex.Message);
    }
}).RequireAuthorization();

app.MapPost("/users/create", async (IUserRepository _userRepository, User newUser, ILogger _logger) => {
    try
    {
        await _userRepository.Create(newUser);
        return Results.NoContent();
    }
    catch(FileNotFoundException ex)
    {
        _logger.LogInformation(ex.ToString());
        return Results.Problem("Server error", statusCode: 500);
    }
    catch(Exception ex)
    {
        _logger.LogWarning(ex.ToString());
        return Results.Problem(ex.Message);
    }
}).RequireAuthorization();

app.MapPut("/users/update/{userId}", async (IUserRepository _userRepository, int? userId, User userToUpdate, ILogger _logger) => {
    try
    {
        if(userId == null) return Results.BadRequest("ID cannot be null");
        if(await _userRepository.GetBy(userId ?? 0) == null) return Results.NotFound($"Not exists user with ID:{int.MinValue}");

        await _userRepository.Update(userToUpdate);
        return Results.Ok("User updated successfully");
    }
    catch(FileNotFoundException ex)
    {
        _logger.LogInformation(ex.ToString());
        return Results.Problem("Server error", statusCode: 500);
    }
    catch(Exception ex)
    {
        _logger.LogWarning(ex.ToString());
        return Results.Problem(ex.Message);
    }
}).RequireAuthorization();

app.MapDelete("users/delete/{userId}", async (IUserRepository _userRepository, int? userId, ILogger _logger) => {
    try
    {
        if(userId == null) return Results.BadRequest("ID cannot be null");
        if(await _userRepository.GetBy(userId ?? 0) == null) return Results.NotFound($"Not exists user with ID:{int.MinValue}");

        await _userRepository.Delete(userId ?? 0);
        return Results.Ok("User deleted successfully");
    }
    catch(FileNotFoundException ex)
    {
        _logger.LogInformation(ex.ToString());
        return Results.Problem("Server error", statusCode: 500);
    }
    catch(Exception ex)
    {
        _logger.LogWarning(ex.ToString());
        return Results.Problem(ex.Message);
    }
}).RequireAuthorization();

app.Run();
