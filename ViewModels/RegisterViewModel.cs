using System.ComponentModel.DataAnnotations;

namespace TupiJua.ViewModels
{
    /// <summary>
    /// ViewModel para registro de novo usuário.
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Nome de usuário do novo usuário.
        /// </summary>
        [Required(ErrorMessage = "O campo Nome de Usuário é obrigatório.")]
        public string Username { get; set; } = null!;

        /// <summary>
        /// Email do novo usuário.
        /// </summary>
        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Senha do novo usuário.
        /// </summary>
        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        /// <summary>
        /// Confirmação da senha do novo usuário.
        /// </summary>
        [Required(ErrorMessage = "O campo Confirmar Senha é obrigatório.")]
        [Compare("Password", ErrorMessage = "As senhas não coincidem.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;
    }
}
