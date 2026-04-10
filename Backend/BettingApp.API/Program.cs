using Microsoft.EntityFrameworkCore;
using BettingApp.API.Data;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Servicii de bază ---
builder.Services.AddControllers(); // Importante pentru [ApiController]
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- 2. Configurare Bază de Date (SQLite pentru început) ---
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite("Data Source=betting.db"));

// --- 3. Configurare CORS (Să poată Noris să se conecteze) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// --- 4. Pipeline-ul de execuție ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Activează CORS înainte de maparea controllerelor
app.UseCors("AllowAngular");

app.UseAuthorization();

app.MapControllers(); // Asta va găsi AuthController-ul automat

app.Run();