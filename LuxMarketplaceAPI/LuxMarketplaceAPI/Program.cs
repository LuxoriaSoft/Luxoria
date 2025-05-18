var builder = WebApplication.CreateBuilder(args);

/**
 * ENVIRONMENT VARIABLES
*/
string LUXORIA_WEBSITE = builder.Configuration["Luxoria:Website"]
                         ?? throw new ArgumentNullException("Luxoria:Website");

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add controllers
builder.Services.AddControllers();

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Redirect HTTP requests to HTTPS
app.UseHttpsRedirection();

// Map Controllers
app.MapControllers();

// Default Route '/' redirect to luxoria's website
app.MapGet("/", () => Results.Redirect(LUXORIA_WEBSITE));

app.Run();
