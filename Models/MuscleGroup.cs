using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TupiJua.Models
{
    /// <summary>
    /// Grupo muscular (ex.: Peitoral).
    /// </summary>
    public class MuscleGroup
    {
        /// <summary>
        /// Identificador do grupo muscular.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Nome do grupo muscular (ex.: Peitoral).
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Descrição do grupo muscular.
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Cor associada ao grupo muscular em hexadecimal (ex.: "1aff2b").
        /// </summary>
        [MaxLength(6)]
        [RegularExpression(@"^[0-9a-fA-F]{6}$", ErrorMessage = "A cor deve ser um código hexadecimal de 6 caracteres.")]
        public string? Color { get; set; }

        /// <summary>
        /// Exercícios associados a este grupo muscular.
        /// </summary>
        public virtual ICollection<ExerciseMuscleGroup> ExerciseMuscleGroups { get; set; } = new List<ExerciseMuscleGroup>();
    }
}