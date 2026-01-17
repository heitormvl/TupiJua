using System.ComponentModel.DataAnnotations;

namespace TupiJua.ViewModels
{
    /// <summary>
    /// ViewModel para login do usuário.
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Nome de usuário ou email do usuário.
        /// </summary>
        [Display(Name = "Nome de Usuário ou Email")]
        [Required(ErrorMessage = "O campo Nome de Usuário ou Email é obrigatório.")]
        public string Username { get; set; } = null!;

        /// <summary>
        /// Senha do usuário.
        /// </summary>
        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        /// <summary>
        /// Indica se o usuário deseja que a sessão seja lembrada.
        /// </summary>
        public bool RememberMe { get; set; }
    }
}
