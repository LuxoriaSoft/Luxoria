using LuxAPI.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using LuxAPI.Hubs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LuxAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add necessary services
builder.Services.AddHttpClient("DefaultClient"); // Named client

/**
 * ENVIRONMENT VARIABLES
*/
// Database connection string
string DB_DEFAULT_CONNECTION = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Database connection string is not set.");
// Frontend URL
string FRONT_URI = builder.Configuration["URI:FrontEnd"]
    ?? throw new Exception("Frontend URL is not set.");

// Backend URL
string BACKEND_URI = builder.Configuration["URI:Backend"]
    ?? throw new Exception("Backend URL is not set.");

// JWT settings
// JWT Key
string JWT_KEY = builder.Configuration["Jwt:Key"]
    ?? throw new Exception("JWT Key is not set.");
// JWT Issuer
string JWT_ISSUER = builder.Configuration["Jwt:Issuer"]
    ?? throw new Exception("JWT Issuer is not set.");
// JWT Audience
string JWT_AUDIENCE = builder.Configuration["Jwt:Audience"]
    ?? throw new Exception("JWT Audience is not set.");

// Check SMTP settings
if (string.IsNullOrEmpty(builder.Configuration["Smtp:Host"]) ||
    string.IsNullOrEmpty(builder.Configuration["Smtp:Port"]) ||
    string.IsNullOrEmpty(builder.Configuration["Smtp:Username"]) ||
    string.IsNullOrEmpty(builder.Configuration["Smtp:Password"]))
{
    throw new Exception("SMTP settings are not properly configured.");
}

// Add services
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(DB_DEFAULT_CONNECTION));

builder.Services.AddSingleton<MinioService>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddHostedService<CleanupExpiredRegistrations>();


// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWT_KEY)),
        ValidateIssuer = true,
        ValidIssuer = JWT_ISSUER,
        ValidateAudience = true,
        ValidAudience = JWT_AUDIENCE,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        NameClaimType = ClaimTypes.Email
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

// Add controllers
builder.Services.AddControllers();

// Add SignalR
builder.Services.AddSignalR();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(FRONT_URI) // Allow Frontend URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Allow credentials
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LuxAPI", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] { }
        }
    });
});

// Enable OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Run migrations using Microsoft EF
using (var scope = builder.Services.BuildServiceProvider().CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Run pending migrations
    dbContext.Database.Migrate();

    // Seed default Client if empty
    if (!dbContext.Clients.Any())
    {
        dbContext.Clients.Add(new LuxAPI.Models.Client
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            ClientSecret = Guid.NewGuid().ToString("N"),
            RedirectUri = "http://localhost:5001/callback",
            IsDefault = true
        });

        dbContext.SaveChanges();
    }
}


// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Disabled for Kubernetes usage ONLY
//app.UseHttpsRedirection();

// Enable CORS
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Hub mapping to "/hubs/chat"
// This is where the SignalR hub is registered in the application pipeline
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
