using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TupiJua.Models
{
    /// <summary>
    /// Exercício (ex.: Supino Reto).
    /// </summary>
    public class Exercise
    {
        /// <summary>
        /// Identificador do exercício.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Nome do exercício (ex.: Supino Reto).
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Descrição do exercício.
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Exercícios registrados associados a este exercício.
        /// </summary>
        public virtual ICollection<LoggedExercise> LoggedExercises { get; set; } = new List<LoggedExercise>();

        /// <summary>
        /// Grupos musculares associados a este exercício.
        /// </summary>
        public virtual ICollection<ExerciseMuscleGroup> ExerciseMuscleGroups { get; set; } = new List<ExerciseMuscleGroup>();
    }
}