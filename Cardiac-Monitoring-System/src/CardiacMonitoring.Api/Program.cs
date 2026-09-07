using System.Text;
using CardiacMonitoring.Api.Data; 
using CardiacMonitoring.Api.Identity;
using CardiacMonitoring.Api.Repositories;
using CardiacMonitoring.Api.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; 
using CardiacMonitoring.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

// ---- Database ----
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

    // Sprint 3, Day 1: enable query logging in development so we can
    // see the exact SQL generated and count queries per request — the
    // only reliable way to diagnose N+1 problems is to watch the actual
    // queries, not to guess from reading the C# code.
    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
               .EnableSensitiveDataLogging()
               .EnableDetailedErrors();
    }
});

// ---- Identity ----
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ---- JWT Authentication ----
var jwtKey = builder.Configuration["Jwt:Key"]!;
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// ---- Application services ----
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IRiskEvaluator, CardiacRiskEvaluator>(); 
builder.Services.AddScoped<IVitalSignService, VitalSignService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>(); 

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ---- CORS ----
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://cardiac-frontend.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ---- Rate Limiting ----
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("general", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
    });

    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
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

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Sprint 3, Day 1: realistic seed volume — 2 patients with 3 readings
    // each is invisible to N+1 problems. 20 patients with 15+ readings each
    // makes them immediately visible in the query log as a flood of SQL
    // statements instead of the expected 1-2 queries.
    if (!context.Patients.Any())
    {
        var rng = new Random(42);

        var patientData = new[]
        {
            ("Layla Ahmad",      new DateTime(1968,  4, 12), "Female"),
            ("Omar Khalil",      new DateTime(1975, 11,  3), "Male"),
            ("Sara Hassan",      new DateTime(1982,  7, 22), "Female"),
            ("Ahmad Nasser",     new DateTime(1955,  3,  8), "Male"),
            ("Rania Yousef",     new DateTime(1990,  1, 30), "Female"),
            ("Khalid Mansour",   new DateTime(1963,  9, 14), "Male"),
            ("Nour Al-Deen",     new DateTime(1978,  5, 19), "Female"),
            ("Tariq Ibrahim",    new DateTime(1948, 12,  1), "Male"),
            ("Hana Saleh",       new DateTime(1985,  8,  6), "Female"),
            ("Yousef Al-Rashid", new DateTime(1970,  2, 27), "Male"),
            ("Dina Khalil",      new DateTime(1993,  6, 11), "Female"),
            ("Majed Qasim",      new DateTime(1960, 10, 18), "Male"),
            ("Lina Farouk",      new DateTime(1987,  4,  3), "Female"),
            ("Bilal Amin",       new DateTime(1952,  7, 29), "Male"),
            ("Aya Mustafa",      new DateTime(1995,  3, 15), "Female"),
            ("Faris Al-Omari",   new DateTime(1967, 11, 22), "Male"),
            ("Sana Haddad",      new DateTime(1980,  9,  7), "Female"),
            ("Ramzi Barakat",    new DateTime(1944,  1, 14), "Male"),
            ("Mira Aziz",        new DateTime(1988,  6, 25), "Female"),
            ("Ziad Karimi",      new DateTime(1973,  8, 31), "Male"),
        };

        var patients = patientData.Select(p => new CardiacMonitoring.Api.Entities.Patient
        {
            FullName = p.Item1, DateOfBirth = p.Item2, Gender = p.Item3
        }).ToList();

        context.Patients.AddRange(patients);
        context.SaveChanges();

        var vitals      = new List<CardiacMonitoring.Api.Entities.VitalSign>();
        var medications = new List<CardiacMonitoring.Api.Entities.Medication>();
        var appointments= new List<CardiacMonitoring.Api.Entities.Appointment>();

        var medicationNames = new[] { "Lisinopril", "Metoprolol", "Aspirin", "Atorvastatin", "Warfarin", "Amiodarone" };
        var frequencies     = new[] { "Once daily", "Twice daily", "Three times daily", "As needed" };
        var doctors         = new[] { "Dr. Nadia Saleh", "Dr. Khalid Omar", "Dr. Rana Haddad", "Dr. Samir Aziz" };
        var reasons         = new[] { "Cardiac follow-up", "Routine check", "Medication review", "Post-discharge follow-up" };

        foreach (var patient in patients)
        {
            for (int i = 0; i < 15; i++)
            {
                var hr  = rng.Next(38, 145);
                var sbp = rng.Next(78, 195);
                var dbp = rng.Next(50, 110);
                var spo = 85.0 + rng.NextDouble() * 15;

                var risk = (hr > 130 || hr < 40 || sbp > 180 || sbp < 80 || spo < 90)
                    ? CardiacMonitoring.Api.Entities.RiskLevel.Critical
                    : (hr > 100 || sbp > 140 || spo < 95)
                        ? CardiacMonitoring.Api.Entities.RiskLevel.Watch
                        : CardiacMonitoring.Api.Entities.RiskLevel.Normal;

                vitals.Add(new CardiacMonitoring.Api.Entities.VitalSign
                {
                    PatientId               = patient.Id,
                    HeartRateBpm            = hr,
                    SystolicBp              = sbp,
                    DiastolicBp             = dbp,
                    OxygenSaturationPercent = Math.Round(spo, 1),
                    RecordedAtUtc           = DateTime.UtcNow.AddHours(-(i * 8 + rng.Next(1, 5))),
                    RiskLevel               = risk
                });
            }

            for (int i = 0; i < 2; i++)
                medications.Add(new CardiacMonitoring.Api.Entities.Medication
                {
                    PatientId = patient.Id,
                    Name      = medicationNames[rng.Next(medicationNames.Length)],
                    DosageMg  = rng.Next(5, 100) * 5,
                    Frequency = frequencies[rng.Next(frequencies.Length)]
                });

            for (int i = 0; i < 2; i++)
                appointments.Add(new CardiacMonitoring.Api.Entities.Appointment
                {
                    PatientId      = patient.Id,
                    ScheduledAtUtc = DateTime.UtcNow.AddDays(rng.Next(1, 30)),
                    DoctorName     = doctors[rng.Next(doctors.Length)],
                    Reason         = reasons[rng.Next(reasons.Length)]
                });
        }

        context.VitalSigns.AddRange(vitals);
        context.Medications.AddRange(medications);
        context.Appointments.AddRange(appointments);
        context.SaveChanges();
    }

    // Seed Admin account
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<CardiacMonitoring.Api.Identity.ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    const string adminEmail = "admin@cardiac.com";
    if (await userManager.FindByEmailAsync(adminEmail) is null)
    {
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new IdentityRole("Admin"));

        var adminUser = new CardiacMonitoring.Api.Identity.ApplicationUser
        {
            UserName = adminEmail,
            Email    = adminEmail,
            FullName = "System Administrator"
        };

        var result = await userManager.CreateAsync(adminUser, "AdminPass123!");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CardiacMonitoring.Api.Middleware.AuditLoggingMiddleware>();
app.MapControllers();
app.Run(); 

public partial class Program { }
