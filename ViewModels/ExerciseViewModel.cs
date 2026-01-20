using System.ComponentModel.DataAnnotations;

namespace TupiJua.ViewModels
{
    /// <summary>
    /// ViewModel para criação e edição de exercícios
    /// </summary>
    public class ExerciseViewModel
    {
        /// <summary>
        /// Identificador do exercício (usado na edição)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do exercício
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório")]
        [MaxLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Descrição do exercício
        /// </summary>
        [MaxLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres")]
        [Display(Name = "Descrição")]
        public string? Description { get; set; }

        /// <summary>
        /// IDs dos grupos musculares associados
        /// </summary>
        [Display(Name = "Grupos Musculares")]
        public List<int>? MuscleGroupIds { get; set; }

        /// <summary>
        /// IDs dos grupos musculares primários
        /// </summary>
        [Display(Name = "Grupos Musculares Primários")]
        public List<int>? PrimaryMuscleGroupIds { get; set; }
    }
}
