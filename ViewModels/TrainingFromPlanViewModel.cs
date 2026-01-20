using System.ComponentModel.DataAnnotations;

namespace TupiJua.ViewModels
{
    /// <summary>
    /// ViewModel para execução de treino baseado em plano.
    /// </summary>
    public class TrainingFromPlanViewModel
    {
        /// <summary>
        /// ID da sessão de treino iniciada.
        /// </summary>
        [Required]
        public int SessionId { get; set; }

        /// <summary>
        /// ID do plano de treino associado.
        /// </summary>
        [Required]
        public int WorkoutPlanId { get; set; }

        /// <summary>
        /// Nome do plano de treino.
        /// </summary>
        public string? PlanName { get; set; }

        /// <summary>
        /// Lista de exercícios do plano com informações de execução.
        /// </summary>
        public List<PlanExerciseItemViewModel> Exercises { get; set; } = new();
    }

    /// <summary>
    /// ViewModel para cada exercício do plano durante a execução.
    /// </summary>
    public class PlanExerciseItemViewModel
    {
        /// <summary>
        /// ID do exercício.
        /// </summary>
        public int ExerciseId { get; set; }

        /// <summary>
        /// Nome do exercício.
        /// </summary>
        public string ExerciseName { get; set; } = string.Empty;

        /// <summary>
        /// Ordem do exercício no plano.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Séries alvo definidas no plano.
        /// </summary>
        public int TargetSets { get; set; }

        /// <summary>
        /// Repetições alvo definidas no plano.
        /// </summary>
        public string? TargetReps { get; set; }

        /// <summary>
        /// Tempo de descanso recomendado entre séries.
        /// </summary>
        public int RecommendedRestTime { get; set; }

        /// <summary>
        /// Flag indicando se o tempo de descanso está em minutos (true) ou segundos (false).
        /// </summary>
        public bool RestInMinutes { get; set; } = false;

        /// <summary>
        /// Indica se o exercício já foi executado na sessão atual.
        /// </summary>
        public bool IsCompleted { get; set; }
    }
}
