using System.ComponentModel.DataAnnotations;

namespace TupiJua.ViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "Nome de Usuário ou Email")]
        [Required(ErrorMessage = "O campo Nome de Usuário ou Email é obrigatório.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
    }
}
