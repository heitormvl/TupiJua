using System.ComponentModel.DataAnnotations;

namespace TupiJua.ViewModels
{
    /// <summary>
    /// ViewModel para criação e edição de grupos musculares
    /// </summary>
    public class MuscleGroupViewModel
    {
        /// <summary>
        /// Identificador do grupo muscular (usado na edição)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do grupo muscular
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório")]
        [MaxLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Descrição do grupo muscular
        /// </summary>
        [MaxLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres")]
        [Display(Name = "Descrição")]
        public string? Description { get; set; }

        /// <summary>
        /// Cor em formato hexadecimal (sem #)
        /// </summary>
        [MaxLength(6, ErrorMessage = "A cor deve ter 6 caracteres")]
        [RegularExpression(@"^[0-9a-fA-F]{6}$", ErrorMessage = "A cor deve ser um código hexadecimal de 6 caracteres")]
        [Display(Name = "Cor (Hex)")]
        public string? Color { get; set; }
    }
}
