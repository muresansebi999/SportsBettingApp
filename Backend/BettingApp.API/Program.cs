using Microsoft.EntityFrameworkCore;
using BettingApp.API.Data;
using BettingApp.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("acesta-este-un-token-key-foarte-lung-si-sigur-pentru-proiectul-meu-de-betting-2024!")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DataContext>();

    if (!context.Teams.Any())
    {
        var teams = new List<Team>();
        
        var leagueData = new Dictionary<string, List<string>>
        {
            { "SuperLiga", new List<string> { "FCSB", "CFR Cluj", "Universitatea Craiova", "Rapid Bucuresti", "Farul Constanta", "Sepsi OSK", "Universitatea Cluj", "FC Hermannstadt", "UTA Arad", "Petrolul Ploiesti", "Otelul Galati", "Poli Iasi", "Dinamo Bucuresti", "FC Botosani", "Gloria Buzau", "Unirea Slobozia" } },
            { "Premier League", new List<string> { "Arsenal", "Aston Villa", "Bournemouth", "Brentford", "Brighton", "Chelsea", "Crystal Palace", "Everton", "Fulham", "Ipswich Town", "Leicester City", "Liverpool", "Manchester City", "Manchester United", "Newcastle United", "Nottingham Forest", "Southampton", "Tottenham Hotspur", "West Ham United", "Wolverhampton" } },
            { "La Liga", new List<string> { "Athletic Bilbao", "Atletico Madrid", "Barcelona", "Celta Vigo", "Deportivo Alaves", "Espanyol", "Getafe", "Girona", "Las Palmas", "Leganes", "Mallorca", "Osasuna", "Rayo Vallecano", "Real Betis", "Real Madrid", "Real Sociedad", "Real Valladolid", "Sevilla", "Valencia", "Villarreal" } },
            { "Serie A", new List<string> { "AC Milan", "Atalanta", "Bologna", "Cagliari", "Como", "Empoli", "Fiorentina", "Genoa", "Hellas Verona", "Inter Milan", "Juventus", "Lazio", "Lecce", "Monza", "Napoli", "Parma", "AS Roma", "Torino", "Udinese", "Venezia" } },
            { "Bundesliga", new List<string> { "Augsburg", "Bayer Leverkusen", "Bayern Munich", "Bochum", "Borussia Dortmund", "Borussia Monchengladbach", "Eintracht Frankfurt", "Freiburg", "Heidenheim", "Hoffenheim", "Holstein Kiel", "Mainz 05", "RB Leipzig", "St. Pauli", "VfB Stuttgart", "Union Berlin", "Werder Bremen", "Wolfsburg" } },
            { "Ligue 1", new List<string> { "Angers", "Auxerre", "Brest", "Le Havre", "Lens", "Lille", "Monaco", "Montpellier", "Nantes", "Nice", "Olympique Lyonnais", "Olympique de Marseille", "Paris Saint-Germain", "Reims", "Rennes", "Saint-Etienne", "Strasbourg", "Toulouse" } }
        };

        foreach (var league in leagueData)
        {
            foreach (var teamName in league.Value)
            {
                teams.Add(new Team { Name = teamName, League = league.Key });
            }
        }

        context.Teams.AddRange(teams);
        context.SaveChanges();
    }

    if (!context.Matches.Any())
    {
        context.Matches.AddRange(new List<Match>
        {
            new Match { HomeTeam = "FCSB", AwayTeam = "Dinamo Bucuresti", League = "SuperLiga", HomeOdds = 1.85, DrawOdds = 3.40, AwayOdds = 4.20 },
            new Match { HomeTeam = "CFR Cluj", AwayTeam = "Universitatea Cluj", League = "SuperLiga", HomeOdds = 2.10, DrawOdds = 3.20, AwayOdds = 3.40 },
            new Match { HomeTeam = "Arsenal", AwayTeam = "Liverpool", League = "Premier League", HomeOdds = 2.50, DrawOdds = 3.30, AwayOdds = 2.80 },
            new Match { HomeTeam = "Manchester City", AwayTeam = "Chelsea", League = "Premier League", HomeOdds = 1.65, DrawOdds = 3.80, AwayOdds = 5.00 },
            new Match { HomeTeam = "Real Madrid", AwayTeam = "Barcelona", League = "La Liga", HomeOdds = 2.20, DrawOdds = 3.40, AwayOdds = 3.10 },
            new Match { HomeTeam = "Atletico Madrid", AwayTeam = "Sevilla", League = "La Liga", HomeOdds = 1.75, DrawOdds = 3.50, AwayOdds = 4.50 },
            new Match { HomeTeam = "Bayern Munich", AwayTeam = "Borussia Dortmund", League = "Bundesliga", HomeOdds = 1.55, DrawOdds = 4.00, AwayOdds = 5.50 },
            new Match { HomeTeam = "Inter Milan", AwayTeam = "Juventus", League = "Serie A", HomeOdds = 2.30, DrawOdds = 3.20, AwayOdds = 3.00 },
            new Match { HomeTeam = "Paris Saint-Germain", AwayTeam = "Olympique de Marseille", League = "Ligue 1", HomeOdds = 1.45, DrawOdds = 4.20, AwayOdds = 6.50 },
            new Match { HomeTeam = "Napoli", AwayTeam = "AC Milan", League = "Serie A", HomeOdds = 2.40, DrawOdds = 3.30, AwayOdds = 2.90 }
        });

        context.SaveChanges();
    }

    if (!context.Users.Any())
    {
        using var hmac = new System.Security.Cryptography.HMACSHA512();

        context.Users.Add(new User 
        { 
            Username = "admin",
            Email = "admin@betting.com",
            FirstName = "Admin",
            LastName = "Test",
            DateOfBirth = new DateTime(1990, 1, 1),
            Balance = 1000m,
            PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("password123")),
            PasswordSalt = hmac.Key
        });
        
        context.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();