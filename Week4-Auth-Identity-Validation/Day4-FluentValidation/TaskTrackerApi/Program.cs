using TaskTrackerApi.Models;
using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;  
using Microsoft.AspNetCore.Identity; 
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text; 
using Microsoft.OpenApi.Models;
using FluentValidation.AspNetCore;
using FluentValidation;


var builder = WebApplication.CreateBuilder(args);

// AddFluentValidation scans this assembly for every class inheriting
// AbstractValidator<T> and registers each one automatically — no need
// to manually register CreateTaskValidator/UpdateTaskValidator by name.
builder.Services.AddControllers();

// Modern FluentValidation registration (replaces the deprecated
// AddFluentValidation() call): AddFluentValidationAutoValidation() wires
// validators into the MVC pipeline automatically, while
// AddValidatorsFromAssemblyContaining scans this assembly for every
// class inheriting AbstractValidator<T> and registers each one.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Registering Identity with IdentityUser and IdentityRole — this sets
// up user storage, password hashing, and role management, all backed
// by the AppDbContext we just extended from IdentityDbContext.
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>(); 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}) 


.AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        // Temporary diagnostic logging to see exactly why a token gets
        // rejected — without this, a 401 gives no detail about which
        // validation check actually failed.
        /*options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[JWT FAILED] {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"[JWT CHALLENGE] Error: {context.Error}, Description: {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };*/
    });



builder.Services.AddAuthorization(options =>
{
    // A named policy beyond a simple role check — requires both being
    // authenticated AND holding a specific claim, demonstrating that
    // policies can combine multiple requirements, not just one role name.
    options.AddPolicy("CanManageProjects", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin", "User"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your JWT token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        Array.Empty<string>()
    }
});
});


var app = builder.Build(); 

// Temporary seed block: ensures at least one Project exists so Day 4's
// FluentValidation ProjectId-existence rule can be tested end-to-end.
// Safe to remove once manual testing is done — it only inserts if the
// Projects table is empty, so re-running never creates duplicates.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!context.Projects.Any())
    {
        // OwnerId is required (FK-like link to IdentityUser.Id) but any
        // existing user's Id works fine here — this is throwaway test
        // data, not meant to represent real project ownership.
        var anyUser = context.Users.FirstOrDefault();
        context.Projects.Add(new TaskTrackerApi.Models.Project
        {
            Name = "Day 4 Validation Testing",
            Budget = 1000.00m,
            OwnerId = anyUser != null ? anyUser.Id : "seed-placeholder"
        });
        context.SaveChanges();
        Console.WriteLine("[SEED] Test project created.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();