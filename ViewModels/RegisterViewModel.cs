using System.ComponentModel.DataAnnotations;

namespace TupiJua.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "O campo Nome de Usuário é obrigatório.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "O campo Confirmar Senha é obrigatório.")]
        [Compare("Password", ErrorMessage = "As senhas não coincidem.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = null!;
    }
}
