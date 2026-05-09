using BookingBoardGames.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5000");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
        BookingBoardGames.Api.DatabaseSeeder.Seed(dbContext);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during migration: {ex.Message}");
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthorization();

// Browser check: GET /api/ otherwise 404 (controllers are under /api/Games, /api/Users, …)
app.MapGet("/api", () => Results.Json(new
{
    name = "BookingBoardGames.Api",
    swagger = "/swagger",
    hint = "Sample: GET http://localhost:5000/api/games",
}));
app.MapControllers();



app.Run();
