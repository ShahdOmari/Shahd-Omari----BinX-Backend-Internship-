using System.Text;
using CardiacMonitoring.Api.Data;
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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---- Identity ----
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
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

// Registers the global exception handler and ASP.NET Core's built-in
// ProblemDetails service (which also standardizes automatic responses
// like model-validation failures into the same RFC 7807 shape).
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ---- CORS ----
// A named policy allowing only a specific known frontend origin — a
// permissive "allow any origin" policy is convenient in development but
// should never ship to production, since it lets any website's script
// call this API on a logged-in user's behalf.
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
// A stricter limiter on login specifically, since repeated rapid login
// attempts are the clearest sign of a brute-force attack in progress.
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
// Synthetic seed data — ensures the database always has demonstrable
// content for grading/demo purposes without requiring manual setup.
// Idempotent: only seeds if the Patients table is empty, so re-running
// the app never creates duplicates.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!context.Patients.Any())
    {
        var patient1 = new CardiacMonitoring.Api.Entities.Patient
        {
            FullName = "Layla Ahmad", DateOfBirth = new DateTime(1968, 4, 12), Gender = "Female"
        };
        var patient2 = new CardiacMonitoring.Api.Entities.Patient
        {
            FullName = "Omar Khalil", DateOfBirth = new DateTime(1975, 11, 3), Gender = "Male"
        };
        context.Patients.AddRange(patient1, patient2);
        context.SaveChanges();

        context.VitalSigns.AddRange(
            new CardiacMonitoring.Api.Entities.VitalSign
            {
                PatientId = patient1.Id, HeartRateBpm = 78, SystolicBp = 122, DiastolicBp = 80,
                OxygenSaturationPercent = 97, RecordedAtUtc = DateTime.UtcNow.AddHours(-2),
                RiskLevel = CardiacMonitoring.Api.Entities.RiskLevel.Normal
            },
            new CardiacMonitoring.Api.Entities.VitalSign
            {
                PatientId = patient2.Id, HeartRateBpm = 138, SystolicBp = 188, DiastolicBp = 102,
                OxygenSaturationPercent = 88, RecordedAtUtc = DateTime.UtcNow.AddMinutes(-30),
                RiskLevel = CardiacMonitoring.Api.Entities.RiskLevel.Critical
            });

        context.Medications.Add(new CardiacMonitoring.Api.Entities.Medication
        {
            PatientId = patient1.Id, Name = "Lisinopril", DosageMg = 10, Frequency = "Once daily"
        });

        context.Appointments.Add(new CardiacMonitoring.Api.Entities.Appointment
        {
            PatientId = patient2.Id, ScheduledAtUtc = DateTime.UtcNow.AddDays(3),
            DoctorName = "Dr. Nadia Saleh", Reason = "Cardiac follow-up"
        });

        context.SaveChanges();
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
    // HSTS tells browsers to always use HTTPS for this domain going
    // forward. Only enabled outside Development, since it would otherwise
    // block local HTTP testing in the browser.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Order matters: CORS and rate limiting run before authentication/
// authorization, so a disallowed origin or a rate-limited client is
// rejected before the request ever reaches an identity check.
app.UseCors("AllowFrontend");
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run(); 

// Exposed as public so WebApplicationFactory<Program> in the test project
// can reference this entry point — top-level statement Program classes
// are internal by default.
public partial class Program { }
