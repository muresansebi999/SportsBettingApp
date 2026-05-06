using Microsoft.EntityFrameworkCore;
using BettingApp.API.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer; // ADĂUGAT
using Microsoft.IdentityModel.Tokens; // ADĂUGAT
using System.Text; // ADĂUGAT
using BettingApp.API.Services;
using BettingApp.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<TokenService>();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite("Data Source=betting.db"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ==== START COD ADĂUGAT PENTRU AUTENTIFICARE ====
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            // AICI: Verificăm tokenul folosind o cheie secretă (din appsettings.json sau una directă)
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("acesta-este-un-token-key-foarte-lung-si-sigur-pentru-proiectul-meu-de-betting-2024!")),
          ValidateIssuer = false,
            ValidateAudience = false
        };
    });
// ==== END COD ADĂUGAT ====

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthentication(); // ADĂUGAT: Obligatoriu deasupra la UseAuthorization!
app.UseAuthorization();

app.MapControllers();

app.Run();