using LMS.API.Middleware;
using LMS.Application.Common.Settings;
using LMS.Application.Interfaces.Common;
using LMS.Application.Interfaces.Handlers;
using LMS.Application.Interfaces.Repositories;
using LMS.Application.Interfaces.Repositories.Loan;
using LMS.Application.Interfaces.Security;
using LMS.Application.Interfaces.Services;
using LMS.Application.Services.Auth;
using LMS.Application.Services.Common;
using LMS.Infrastructure.Persistence.Context;
using LMS.Infrastructure.Persistence.Repositories.Auth;
using LMS.Infrastructure.Persistence.Repositories.Loan;
using LMS.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Database configuration
builder.Services.AddDbContext<LMSDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LMSDB_LOCAL"))
);
builder.Services.Configure<OtpSettings>(
    builder.Configuration.GetSection("OtpSettings"));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Add services to the container.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUserLoginHistoryRepository, UserLoginHistoryRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();
builder.Services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ILogService, LogService>();
builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ILoanFinancialRepository, LoanFinancialRepository>();

builder.Services.AddControllers();

// ? ADD CORS CONFIGURATION
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUI", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",           // React local dev
                "http://localhost:4200",           // Angular local dev
                "https://yourdomain.com"           // Production domain
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("X-Request-Id");   // Expose custom headers
    });
});

// JWT Authentication configuration
// Configure default authentication scheme for login tokens
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

// Adding Swagger with JWT Authorization support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LMS",
        Version = "v1"
    });

    // Define the security scheme for Bearer tokens in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter only the JWT token; 'Bearer' is added automatically.",
    });

    // Add security requirement to enforce Bearer token use
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        "logs/log-.txt",
        rollingInterval: RollingInterval.Day
    )
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<AuthRateLimitMiddleware>();

app.UseHttpsRedirection();

// ? ADD CORS USAGE (BEFORE UseAuthentication)
app.UseCors("AllowUI");

app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Application is starting...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.Information("Application stopped");
    Log.CloseAndFlush();
}
