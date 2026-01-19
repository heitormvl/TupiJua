using System.ComponentModel.DataAnnotations;

namespace TupiJua.ViewModels
{
    /// <summary>
    /// ViewModel para editar um exercício em um plano de treino.
    /// </summary>
    public class EditWorkoutPlanExerciseViewModel
    {
        /// <summary>
        /// ID do exercício no plano de treino.
        /// </summary>
        public int Id { get; set; }

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
        /// Tempo de descanso entre séries, em segundos.
        /// </summary>
        [Range(0, 3600, ErrorMessage = "O descanso deve ser entre {1} e {2} segundos.")]
        public int RecommendedRestSeconds { get; set; }

        /// <summary>
        /// Ordem do exercício no plano de treino.
        /// </summary>
        [Range(1, 100, ErrorMessage = "A ordem deve ser entre {1} e {2}.")]
        public int Order { get; set; }
    }
}
