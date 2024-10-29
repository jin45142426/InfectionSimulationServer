using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB
{
    public class AppDbContext : DbContext
    {
        public DbSet<DataModel.AccountDb> Accounts { get; set; }
        public DbSet<DataModel.ScoreDb> Scores { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("Server=localhost;Database=InfectionSimulationDb;User=root;Password=1234",
                new MySqlServerVersion(new Version(11, 4, 3)));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // AccountDb 설정
            modelBuilder.Entity<DataModel.AccountDb>()
                .Property(a => a.Id)
                .IsRequired();  // Null 불가
            modelBuilder.Entity<DataModel.AccountDb>()
                .Property(a => a.Name)
                .IsRequired();  // Null 불가
            modelBuilder.Entity<DataModel.AccountDb>()
                .Property(a => a.Pw)
                .IsRequired();  // Null 불가

            // ScoreDb 설정
            modelBuilder.Entity<DataModel.ScoreDb>()
                .Property(s => s.ScoreId)
                .ValueGeneratedOnAdd();  // 자동 증가 설정
            modelBuilder.Entity<DataModel.ScoreDb>()
                .Property(s => s.FinalScore)
                .IsRequired();  // Null 불가
            modelBuilder.Entity<DataModel.ScoreDb>()
                .Property(s => s.FaultCount)
                .IsRequired();  // Null 불가
            modelBuilder.Entity<DataModel.ScoreDb>()
                .Property(s => s.GameDate)
                .IsRequired();  // Null 불가
            modelBuilder.Entity<DataModel.ScoreDb>()
                .Property(s => s.AccountId)
                .IsRequired();  // Null 불가
            modelBuilder.Entity<DataModel.ScoreDb>()
                .HasOne(s => s.Account)
                .WithMany(a => a.Scores)
                .HasForeignKey(s => s.AccountId)
                .IsRequired();  // 외래 키 Null 불가

            base.OnModelCreating(modelBuilder);
        }

        public bool SaveChangesEx()
        {
            try
            {
                SaveChanges();
                return true; // 성공
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaveChanges 실패: {ex.Message}");
                return false; // 실패
            }
        }
    }
}
