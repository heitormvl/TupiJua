using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TupiJua.Models
{
    /// <summary>
    /// Associação N:N entre Plano de Treino e Exercício.
    /// Representa um exercício específico dentro de um plano de treino, incluindo metas sugeridas.
    /// </summary>
    public class WorkoutPlanExercise
    {
        /// <summary>
        /// Identificador da associação.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Identificador do plano de treino (chave estrangeira para WorkoutPlan.Id).
        /// </summary>
        [Required]
        public int WorkoutPlanId { get; set; }
        [ForeignKey(nameof(WorkoutPlanId))]
        public virtual WorkoutPlan? WorkoutPlan { get; set; }

        /// <summary>
        /// Identificador do exercício (chave estrangeira para Exercise.Id).
        /// </summary>
        [Required]
        public int ExerciseId { get; set; }
        [ForeignKey(nameof(ExerciseId))]
        public virtual Exercise? Exercise { get; set; }

        // Metas sugeridas pelo plano

        /// <summary>
        /// Séries alvo para este exercício no plano de treino.
        /// </summary>
        [Display(Name = "Séries Alvo")]
        public int TargetSets { get; set; }

        /// <summary>
        /// Repetições alvo para este exercício no plano de treino.
        /// </summary>
        [Display(Name = "Reps Alvo")]
        [MaxLength(50)]
        public string? TargetReps { get; set; } // Ex: "10-12"

        /// <summary>
        /// Ordem do exercício dentro do plano de treino.
        /// </summary>
        public int Order { get; set; } // Para o usuário organizar a sequência do treino

        /// <summary>
        /// Descanso recomendado entre séries, em segundos.
        /// </summary>
        public int RecommendedRestSeconds { get; set; } // Descanso recomendado entre séries
    }
}