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

app.Run();
