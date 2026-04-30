using Microsoft.EntityFrameworkCore;
using BettingApp.API.Data;
using BettingApp.API.Models; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
        context.Matches.Add(new Match 
        { 
            HomeTeam = "FCSB", 
            AwayTeam = "Dinamo Bucuresti", 
            League = "SuperLiga",
            HomeOdds = 1.85, 
            DrawOdds = 3.40, 
            AwayOdds = 4.20 
        });

        context.SaveChanges();
    }

    if (!context.Users.Any())
    {
        // Folosim un algoritm de securitate ca să transformăm "password123" în Hash și Salt
        using var hmac = new System.Security.Cryptography.HMACSHA512();

        context.Users.Add(new User 
        { 
            Username = "admin",
            FirstName = "Admin",
            LastName = "Test",
            DateOfBirth = new DateTime(1990, 1, 1),
            Balance = 1000m, // Îi dăm și 1000 de lei în cont ca să avem cu ce paria!
            
            // Aici e magia colegului tău:
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
app.UseAuthorization();
app.MapControllers();

app.Run();