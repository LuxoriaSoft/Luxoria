using LuxAPI.DAL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add necessary services
builder.Services.AddHttpClient("DefaultClient"); // Named client

// Add services
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add controllers
builder.Services.AddControllers();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Autorise le frontend sur ce port
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Si vous utilisez des cookies ou des sessions
    });
});

// Enable OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors(); // Ajoutez ceci avant `UseAuthorization`

app.UseAuthorization();
app.MapControllers();
app.Run();
