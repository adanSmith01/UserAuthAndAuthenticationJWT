# Minimal API for user authentication using JWT

## Summary

This API shows how user authentication and authorization works using Json Web Tokens (JWT). For this, I have based myself on a minimal API due to its lightness and flexibility, but also seeking to understand its implementation for a possible real project.

##  Tools

- Visual Studio Code
- .NET 8 SDK

## Packages

To configure the validation of JWTs
```
dotnet add package Microsoft.AspNetCore.Authentication.JWTBearer --version 8.0.11
```


To create and management JWTs
```
dotnet add package Microsoft.IdentityModel.JsonWebTokens --version 8.3.0
```

## **Clarifications**
- This API was created for learning purposes. It serves as a guide to developing a 100% functional REST (or RESTful) API that handles JWTs appropriately and correctly.

- Regarding the user CRUD, there are validation operations that were not added, such as:
    - Validating the email of a created user.
    - Checking if there is more than one user with the same email.