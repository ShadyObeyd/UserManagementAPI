using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using UserManagementAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

var users = new ConcurrentDictionary<int, User>();


// GET - Retrieve all users
app.MapGet("/users", () =>
{
    return Results.Ok(users);
});


// GET - Retrieve user by ID
app.MapGet("/users/{id:int}", (int id) =>
{
    if (users.TryGetValue(id, out var user))
    {
        return Results.Ok(user);
    }

    return Results.NotFound($"User not found.");
});


// POST - Add a new user
app.MapPost("/users", (User newUser) =>
{
    if (!TryValidate(newUser, out var errors))
    {
        return Results.BadRequest(errors);
    }

    var nextId = users.Count == 0
        ? 1
        : users.Values.Max(u => u.Id) + 1;

    newUser.Id = nextId;

    users.TryAdd(newUser.Id, newUser);

    return Results.Created($"/users/{newUser.Id}", newUser);
});


// PUT - Update a user
app.MapPut("/users/{id:int}", (int id, User updatedUser) =>
{
    if (!users.TryGetValue(id, out var existingUser))
    {
        return Results.NotFound("User not found.");
    }

    if (!TryValidate(updatedUser, out var errors))
    {
        return Results.BadRequest(errors);
    }

    existingUser.FirstName = updatedUser.FirstName;
    existingUser.LastName = updatedUser.LastName;
    existingUser.Email = updatedUser.Email;

    return Results.Ok(existingUser);
});


// DELETE - Remove a user
app.MapDelete("/users/{id:int}", (int id) =>
{
    if (users.TryGetValue(id, out var user))
    {
        users.Remove(id, out _);
        return Results.NoContent();
    }

    return Results.NotFound($"User not found.");
});

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<LoggingMiddleware>();

app.Run();

static bool TryValidate(User user, out List<ValidationResult> results)
{
    var context = new ValidationContext(user);

    results = new List<ValidationResult>();

    return Validator.TryValidateObject(user, context, results, true);
}