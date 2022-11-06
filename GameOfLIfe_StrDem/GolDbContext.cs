using GameOfLIfe_StrDem.Models.DB;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace GameOfLIfe_StrDem
{
    public class GolDbContext : DbContext
    {
        public DbSet<GameRecord> GameRecords { get; set; }

        public GolDbContext(DbContextOptions<GolDbContext> options) : base(options)
        {
            Database.EnsureCreated();

            RecurringJob.AddOrUpdate("ClearGameRecords", () => ClearRecords(), Cron.Minutely());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public void ClearRecords()
        {
            if (GameRecords.Any())
            {
                GameRecords.RemoveRange(GameRecords);
                SaveChangesAsync();
            }
        }
    }
}
