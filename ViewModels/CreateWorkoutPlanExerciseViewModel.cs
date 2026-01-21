using System.ComponentModel.DataAnnotations;

namespace TupiJua.ViewModels
{
    /// <summary>
    /// ViewModel para criar um exercício em um plano de treino.
    /// </summary>
    public class CreateWorkoutPlanExerciseViewModel
    {
        /// <summary>
        /// Identificador do exercício.
        /// </summary>
        public int ExerciseId { get; set; }

        /// <summary>
        /// Quantidade de séries totais.
        /// </summary>
        [Required]
        [Range(1, 100, ErrorMessage = "As séries devem ser entre {1} e {2}.")]
        public int TargetSets { get; set; }

        /// <summary>
        /// Repetições: pode ser fixo (ex.: "12") ou faixa (ex.: "10-12").
        /// </summary>
        [Required]
        [MaxLength(20)]
        [RegularExpression(@"^\d+(\s*-\s*\d+)?$", ErrorMessage = "Deve ser um número (ex.: '12') ou faixa (ex.: '10-12').")]
        public string? TargetReps { get; set; }

        /// <summary>
        /// Tempo de descanso recomendado entre séries.
        /// </summary>
        [Range(0, 3600, ErrorMessage = "O descanso deve ser entre {1} e {2}.")]
        public int RecommendedRestTime { get; set; }

        /// <summary>
        /// Flag indicando se o tempo de descanso está em minutos (true) ou segundos (false).
        /// </summary>
        public bool RestInMinutes { get; set; } = false;

        /// <summary>
        /// Ordem do exercício no plano de treino.
        /// </summary>
        [Range(1, 100, ErrorMessage = "A ordem deve ser entre {1} e {2}.")]
        public int Order { get; set; }
    }
}
