using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FitnessCenterWebApplication.Models.Entities;

namespace FitnessCenterWebApplication.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSet tanımlamaları
        public DbSet<GymCenter> GymCenters { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<TrainerAvailability> TrainerAvailabilities { get; set; }
        public DbSet<TrainerService> TrainerServices { get; set; }
        public DbSet<WorkoutPlan> WorkoutPlans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // GymCenter Configuration
            modelBuilder.Entity<GymCenter>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(250);
                entity.HasIndex(e => e.Name);
            });

            // Service Configuration
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.GymCenter)
                    .WithMany(g => g.Services)
                    .HasForeignKey(e => e.GymCenterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Trainer Configuration
            modelBuilder.Entity<Trainer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);

                entity.HasOne(e => e.GymCenter)
                    .WithMany(g => g.Trainers)
                    .HasForeignKey(e => e.GymCenterId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                    .WithOne(u => u.Trainer)
                    .HasForeignKey<Trainer>(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Member Configuration
            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Height).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Weight).HasColumnType("decimal(5,2)");

                entity.HasOne(e => e.User)
                    .WithOne(u => u.Member)
                    .HasForeignKey<Member>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Email);
            });

            // Appointment Configuration
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Member)
                    .WithMany(m => m.Appointments)
                    .HasForeignKey(e => e.MemberId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Trainer)
                    .WithMany(t => t.Appointments)
                    .HasForeignKey(e => e.TrainerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Service)
                    .WithMany(s => s.Appointments)
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => new { e.TrainerId, e.AppointmentDate, e.StartTime });
                entity.HasIndex(e => new { e.MemberId, e.AppointmentDate });
            });

            // TrainerAvailability Configuration
            modelBuilder.Entity<TrainerAvailability>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Trainer)
                    .WithMany(t => t.Availabilities)
                    .HasForeignKey(e => e.TrainerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.TrainerId, e.DayOfWeek });
            });

            // TrainerService Configuration (Many-to-Many)
            modelBuilder.Entity<TrainerService>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Trainer)
                    .WithMany(t => t.TrainerServices)
                    .HasForeignKey(e => e.TrainerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Service)
                    .WithMany(s => s.TrainerServices)
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Aynı antrenör aynı hizmeti birden fazla kez alamaz
                entity.HasIndex(e => new { e.TrainerId, e.ServiceId }).IsUnique();
            });

            // WorkoutPlan Configuration
            modelBuilder.Entity<WorkoutPlan>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);

                entity.HasOne(e => e.Member)
                    .WithMany(m => m.WorkoutPlans)
                    .HasForeignKey(e => e.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ApplicationUser Configuration
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
            });
        }
    }
}