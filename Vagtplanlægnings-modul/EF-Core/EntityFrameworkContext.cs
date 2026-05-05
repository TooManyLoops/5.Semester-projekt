using System.Reflection.Metadata;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;



namespace Vagtplanlægnings_modul.EF_Core
{
    public class EntityFrameworkContext : DbContext
    {
        
        public EntityFrameworkContext(DbContextOptions<EntityFrameworkContext> options)
            : base(options) { }
        
        public DbSet<TestModel> TModel { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var ConnectionString = "Server = localhost,1433; Database = Timegrip; User Id = sa; Password = Password123!; TrustServerCertificate = True; ConnectRetryCount = 0;";
            optionsBuilder.UseSqlServer(
                ConnectionString, o => o.UseCompatibilityLevel(160));
        }
    }
}
