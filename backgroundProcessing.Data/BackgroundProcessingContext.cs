
using BackgroundProcessing.Domain;
using Microsoft.EntityFrameworkCore;

namespace BackgroundProcessing.Data
{
    public class BackgroundProcessingContext : DbContext
    {
        public BackgroundProcessingContext(DbContextOptions<BackgroundProcessingContext> options) : base(options)
        {
            
        }

        public DbSet<DataStoreItem> DataStore { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultContainer("AllInOne");
            modelBuilder.HasManualThroughput(10000);

            modelBuilder.Entity<DataStoreItem>()
                .ToContainer(nameof(DataStoreItem))
                .HasNoDiscriminator()
                .HasDefaultTimeToLive(60)
                .HasPartitionKey(item => item.StringState) // For CosmosDb partition key must be string
                .HasKey(item => item.Id); 

            base.OnModelCreating(modelBuilder);
        }
    }
}
