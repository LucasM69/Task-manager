using Backend;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite($"Data Source={Path.Combine(builder.Environment.ContentRootPath, "tasks.db")}"));

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

app.UseCors();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Meta.Any(m => m.Key == "schema_version"))
    {
        db.Meta.Add(new MetaEntry { Key = "schema_version", Value = "1" });
        db.SaveChanges();
    }

    if (!db.Tasks.Any())
    {
        var now = DateTime.UtcNow;
        db.Tasks.AddRange(
            new TaskItem { Id = Guid.NewGuid().ToString(), Title = "Setup Angular project with Signals",    Completed = true,  CreatedAt = now.AddMinutes(-4) },
            new TaskItem { Id = Guid.NewGuid().ToString(), Title = "Build the task store with signal()",    Completed = true,  CreatedAt = now.AddMinutes(-3) },
            new TaskItem { Id = Guid.NewGuid().ToString(), Title = "Add inline edit with keyboard support", Completed = false, CreatedAt = now.AddMinutes(-2) },
            new TaskItem { Id = Guid.NewGuid().ToString(), Title = "Implement filter with URL sync",        Completed = false, CreatedAt = now.AddMinutes(-1) },
            new TaskItem { Id = Guid.NewGuid().ToString(), Title = "Write unit tests for computed signals",  Completed = false, CreatedAt = now }
        );
        db.SaveChanges();
    }
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapGet("/api/tasks", async (AppDbContext db) =>
    await db.Tasks.OrderByDescending(t => t.CreatedAt).ToListAsync());

app.MapPost("/api/tasks", async (AppDbContext db, CreateTaskRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Title))
        return Results.Problem(detail: "Title cannot be empty.", statusCode: 400, title: "Validation failed");

    if (req.Title.Length > 200)
        return Results.Problem(detail: "Title cannot exceed 200 characters.", statusCode: 400, title: "Validation failed");

    var task = new TaskItem
    {
        Id = Guid.NewGuid().ToString(),
        Title = req.Title.Trim(),
        Completed = false,
        CreatedAt = DateTime.UtcNow
    };
    db.Tasks.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/api/tasks/{task.Id}", task);
});

app.MapPatch("/api/tasks/{id}/title", async (AppDbContext db, string id, UpdateTitleRequest req) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task is null)
        return Results.Problem(detail: $"Task '{id}' not found.", statusCode: 404, title: "Not found");

    if (string.IsNullOrWhiteSpace(req.Title))
        return Results.Problem(detail: "Title cannot be empty.", statusCode: 400, title: "Validation failed");

    if (req.Title.Length > 200)
        return Results.Problem(detail: "Title cannot exceed 200 characters.", statusCode: 400, title: "Validation failed");

    task.Title = req.Title.Trim();
    await db.SaveChangesAsync();
    return Results.Ok(task);
});

app.MapPatch("/api/tasks/{id}/complete", async (AppDbContext db, string id) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task is null)
        return Results.Problem(detail: $"Task '{id}' not found.", statusCode: 404, title: "Not found");

    task.Completed = !task.Completed;
    await db.SaveChangesAsync();
    return Results.Ok(task);
});

app.MapDelete("/api/tasks/{id}", async (AppDbContext db, string id) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task is null)
        return Results.Problem(detail: $"Task '{id}' not found.", statusCode: 404, title: "Not found");

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

record CreateTaskRequest(string Title);
record UpdateTitleRequest(string Title);
