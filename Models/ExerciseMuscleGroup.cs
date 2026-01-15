using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TupiJua.Models
{
    /// <summary>
    /// Associação N:N entre Exercício e Grupo Muscular.
    /// Representa quais grupos musculares participam de um exercício e se são primários.
    /// </summary>
    public class ExerciseMuscleGroup
    {
        /// <summary>
        /// Identificador do exercício.
        /// </summary>
        [Key]
        [Column(Order = 0)]
        public int ExerciseId { get; set; }

        /// <summary>
        /// Identificador do grupo muscular.
        /// </summary>
        [Key]
        [Column(Order = 1)]
        public int MuscleGroupId { get; set; }

        /// <summary>
        /// Indica se o grupo muscular é primário no exercício.
        /// </summary>
        public bool IsPrimary { get; set; } = false;

        /// <summary>
        /// Navegação para o exercício associado.
        /// </summary>
        [ForeignKey(nameof(ExerciseId))]
        public Exercise Exercise { get; set; } = null!;

        /// <summary>
        /// Navegação para o grupo muscular associado.
        /// </summary>
        [ForeignKey(nameof(MuscleGroupId))]
        public MuscleGroup MuscleGroup { get; set; } = null!;
    }
}