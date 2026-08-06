using TaskTrackerApi.Models;
using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data; 


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build(); 

// Seed a default User and Project on startup, if none exist yet —
// needed so Task creation has valid foreign keys to reference during
// manual testing.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!context.Users.Any())
    {
        var user = new User { Name = "Shahd Omari", Email = "shahd@binx.com" };
        context.Users.Add(user);
        context.SaveChanges();

        var project = new Project { Name = "BinX API", Budget = 5000, OwnerId = user.Id };
        context.Projects.Add(project);
        context.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();