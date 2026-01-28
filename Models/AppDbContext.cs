using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TupiJua.Models
{
    /// <summary>
    /// Representa o contexto do banco de dados para o aplicativo TupiJua.
    /// Herda de IdentityDbContext para incluir suporte a identidade de usuários.
    /// </summary>
    public class AppDbContext : IdentityDbContext<User>
    {
        /// <summary>
        /// Inicializa uma nova instância da classe <see cref="AppDbContext"/>.
        /// </summary>
        /// <param name="options">As opções a serem usadas por um <see cref="DbContext"/>.</param>
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Obtém ou define o DbSet para entidades Exercise.
        /// </summary>
        public DbSet<Exercise> Exercises { get; set; }

        /// <summary>
        /// Obtém ou define o DbSet para entidades ExerciseMuscleGroup.
        /// </summary>
        public DbSet<ExerciseMuscleGroup> ExerciseMuscleGroups { get; set; }

        /// <summary>
        /// Obtém ou define o DbSet para entidades LoggedExercise.
        /// </summary>
        public DbSet<LoggedExercise> LoggedExercises { get; set; }

        /// <summary>
        /// Obtém ou define o DbSet para entidades MuscleGroup.
        /// </summary>
        public DbSet<MuscleGroup> MuscleGroups { get; set; }

        /// <summary>
        /// Obtém ou define o DbSet para entidades WorkoutPlan.
        /// </summary>
        public DbSet<WorkoutPlan> WorkoutPlans { get; set; }

        /// <summary>
        /// Obtém ou define o DbSet para entidades WorkoutSession.
        /// </summary>
        public DbSet<WorkoutSession> WorkoutSessions { get; set; }

        /// <summary>
        /// Configura os relacionamentos do modelo, índices e comportamentos de exclusão.
        /// </summary>
        /// <param name="modelBuilder">O construtor utilizado para criar o modelo deste contexto.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Tenta identificar o fuso de SP independente do SO
            TimeZoneInfo saoPauloTimeZone;
            try
            {
                // Tenta o nome Windows, se falhar tenta o nome IANA (Linux/macOS)
                saoPauloTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                saoPauloTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }

            // 2. Criar o conversor tratando o Kind corretamente
            // Importante: SpecifyKind evita que o .NET tente converter algo que ele já acha que é local
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(v, DateTimeKind.Unspecified), saoPauloTimeZone), // Para o Banco
                v => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(v, DateTimeKind.Utc), saoPauloTimeZone) // Para o App
            );

            // 3. Aplicação Global
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

                foreach (var property in properties)
                {
                    modelBuilder.Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion(dateTimeConverter);
                }
            }

            // Configura relacionamentos e comportamentos de exclusão
            modelBuilder.Entity<WorkoutPlan>()
                .HasMany(wp => wp.WorkoutSessions)
                .WithOne(ws => ws.WorkoutPlan)
                .HasForeignKey(ws => ws.WorkoutPlanId)
                .OnDelete(DeleteBehavior.SetNull); // Preserva o histórico mesmo se o plano for excluído

            modelBuilder.Entity<WorkoutSession>()
                .HasMany(ws => ws.LoggedExercises)
                .WithOne(le => le.WorkoutSession)
                .HasForeignKey(le => le.WorkoutSessionId)
                .OnDelete(DeleteBehavior.Cascade); // Evita dados órfãos (exercícios sem sessão pai)

            modelBuilder.Entity<User>()
                .HasMany(u => u.WorkoutPlans)
                .WithOne(wp => wp.User)
                .HasForeignKey(wp => wp.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Limpeza total ao excluir a conta

            modelBuilder.Entity<User>()
                .HasMany(u => u.WorkoutSessions)
                .WithOne(ws => ws.User)
                .HasForeignKey(ws => ws.UserId)
                .OnDelete(DeleteBehavior.NoAction); // Limpeza ocorre via WorkoutPlan ou manualmente

            modelBuilder.Entity<Exercise>()
                .HasMany(e => e.LoggedExercises)
                .WithOne(le => le.Exercise)
                .HasForeignKey(le => le.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict); // Impede excluir exercício base se houver registro

            // Configura relacionamentos de ExerciseMuscleGroup (N:N com IsPrimary)
            modelBuilder.Entity<ExerciseMuscleGroup>()
                .HasKey(emg => new { emg.ExerciseId, emg.MuscleGroupId });

            modelBuilder.Entity<ExerciseMuscleGroup>()
                .HasOne(emg => emg.Exercise)
                .WithMany(e => e.ExerciseMuscleGroups)
                .HasForeignKey(emg => emg.ExerciseId);

            modelBuilder.Entity<ExerciseMuscleGroup>()
                .HasOne(emg => emg.MuscleGroup)
                .WithMany(mg => mg.ExerciseMuscleGroups)
                .HasForeignKey(emg => emg.MuscleGroupId);

            // Índice composto para evitar mais de um músculo primário por exercício
            modelBuilder.Entity<ExerciseMuscleGroup>()
                .HasIndex(emg => new { emg.ExerciseId })
                .IsUnique()
                .HasFilter("[IsPrimary] = 1"); // Apenas um registro com IsPrimary true por exercício

            // Índices adicionais para performance (campos comuns de consulta)
            modelBuilder.Entity<WorkoutSession>()
                .HasIndex(ws => ws.UserId);

            modelBuilder.Entity<WorkoutPlan>()
                .HasIndex(wp => wp.UserId);

            modelBuilder.Entity<LoggedExercise>()
                .HasIndex(le => le.WorkoutSessionId);

            modelBuilder.Entity<LoggedExercise>()
                .HasIndex(le => le.ExerciseId);
        }
    }
}