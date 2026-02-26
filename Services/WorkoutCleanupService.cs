using Microsoft.EntityFrameworkCore;
using TupiJua.Helpers;
using TupiJua.Models;

namespace TupiJua.Services
{
    /// <summary>
    /// Serviço em segundo plano que executa diariamente à meia-noite para manutenção das sessões de treino.
    /// <para>
    /// Para cada sessão não finalizada com mais de 24 horas:
    /// <list type="bullet">
    ///   <item><description>Se estiver vazia (sem exercícios registrados), é excluída.</description></item>
    ///   <item><description>Se possuir exercícios registrados, é finalizada automaticamente.</description></item>
    /// </list>
    /// </para>
    /// </summary>
    public class WorkoutCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<WorkoutCleanupService> _logger;

        /// <summary>
        /// Inicializa uma nova instância de <see cref="WorkoutCleanupService"/>.
        /// </summary>
        /// <param name="scopeFactory">Fábrica de escopos para resolução de serviços com escopo (como o DbContext).</param>
        /// <param name="logger">Logger para registro de eventos do serviço.</param>
        public WorkoutCleanupService(IServiceScopeFactory scopeFactory, ILogger<WorkoutCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WorkoutCleanupService iniciado.");

            await RunCleanupAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = CalculateDelayUntilMidnight();
                _logger.LogInformation("Próxima limpeza agendada em {Delay}.", delay);

                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    await RunCleanupAsync(stoppingToken);
                }
            }
        }

        /// <summary>
        /// Executa a limpeza das sessões de treino não finalizadas com mais de 24 horas.
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        private async Task RunCleanupAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando limpeza de sessões de treino às {Time}.", DateTimeHelper.Now);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var cutoff = DateTimeHelper.Now.AddHours(-24);

                var staleSessions = await db.WorkoutSessions
                    .Include(s => s.LoggedExercises)
                    .Where(s => !s.IsCompleted && s.Date < cutoff)
                    .ToListAsync(cancellationToken);

                int deleted = 0;
                int completed = 0;

                foreach (var session in staleSessions)
                {
                    if (!session.LoggedExercises.Any())
                    {
                        db.WorkoutSessions.Remove(session);
                        deleted++;
                    }
                    else
                    {
                        session.IsCompleted = true;
                        completed++;
                    }
                }

                await db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Limpeza concluída: {Deleted} sessão(ões) vazia(s) excluída(s), {Completed} sessão(ões) finalizada(s).",
                    deleted, completed);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Erro durante a limpeza das sessões de treino.");
            }
        }

        /// <summary>
        /// Calcula o tempo restante até a próxima meia-noite.
        /// </summary>
        /// <returns>Um <see cref="TimeSpan"/> representando o atraso até 00:00 do dia seguinte.</returns>
        private static TimeSpan CalculateDelayUntilMidnight()
        {
            var now = DateTimeHelper.Now;
            var nextMidnight = DateTimeHelper.Today.AddDays(1);
            return nextMidnight - now;
        }
    }
}
