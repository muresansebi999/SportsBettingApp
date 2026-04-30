using BettingApp.API.Models;
using Microsoft.EntityFrameworkCore;
namespace BettingApp.API.Data {
    public class DataContext : DbContext {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Team> Teams{ get; set; }
        public DbSet<Match> Matches { get; set; }
        
    }
}
