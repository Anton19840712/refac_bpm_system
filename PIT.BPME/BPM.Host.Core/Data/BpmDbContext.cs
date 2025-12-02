using BPME.BPM.Host.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BPME.BPM.Host.Core.Data
{
    /// <summary>
    /// Контекст базы данных BPM Engine.
    ///
    /// Хранит:
    /// - Конфигурации процессов
    /// - Шаги процессов
    /// - История выполнения процессов
    /// </summary>
    public class BpmDbContext : DbContext
    {
        /// <summary>
        /// Создаёт контекст БД
        /// </summary>
        public BpmDbContext(DbContextOptions<BpmDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Конфигурации процессов
        /// </summary>
        public DbSet<ProcessConfigEntity> ProcessConfigs { get; set; }

        /// <summary>
        /// Конфигурации шагов
        /// </summary>
        public DbSet<StepConfigEntity> StepConfigs { get; set; }

        /// <summary>
        /// Экземпляры выполненных процессов (история)
        /// </summary>
        public DbSet<ProcessInstanceEntity> ProcessInstances { get; set; }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация ProcessConfig
            modelBuilder.Entity<ProcessConfigEntity>(entity =>
            {
                entity.ToTable("process_configs");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.PublicId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(e => e.PublicId)
                    .IsUnique();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                // Settings хранится как JSON
                entity.Property(e => e.SettingsJson)
                    .HasColumnType("jsonb");

                // Связь с шагами
                entity.HasMany(e => e.Steps)
                    .WithOne(s => s.ProcessConfig)
                    .HasForeignKey(s => s.ProcessConfigId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Конфигурация StepConfig
            modelBuilder.Entity<StepConfigEntity>(entity =>
            {
                entity.ToTable("step_configs");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.PublicId)
                    .IsRequired()
                    .HasMaxLength(100);

                // Уникальность PublicId в рамках процесса
                entity.HasIndex(e => new { e.ProcessConfigId, e.PublicId })
                    .IsUnique();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.StepType)
                    .IsRequired()
                    .HasMaxLength(50);

                // NextStepIds и Settings как JSON
                entity.Property(e => e.NextStepIdsJson)
                    .HasColumnType("jsonb");

                entity.Property(e => e.SettingsJson)
                    .HasColumnType("jsonb");
            });

            // Конфигурация ProcessInstance
            modelBuilder.Entity<ProcessInstanceEntity>(entity =>
            {
                entity.ToTable("process_instances");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.InstanceId)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(e => e.InstanceId)
                    .IsUnique();

                entity.HasIndex(e => e.CorrelationId);

                entity.Property(e => e.ProcessPublicId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(e => e.ProcessPublicId);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasIndex(e => e.Status);

                entity.Property(e => e.InputArgumentsJson)
                    .HasColumnType("jsonb");

                entity.Property(e => e.OutputResultJson)
                    .HasColumnType("jsonb");

                entity.Property(e => e.ErrorMessage)
                    .HasMaxLength(2000);

                entity.Property(e => e.Source)
                    .HasMaxLength(100);

                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}
