namespace TupiJua.ViewModels
{
    /// <summary>
    /// ViewModel para exibição de informações do usuário.
    /// </summary>
    public class UserViewModel
    {
        /// <summary>
        /// Nome de usuário do usuário.
        /// </summary>
        public string Username { get; set; } = null!;

        /// <summary>
        /// Email do usuário.
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Peso do usuário em quilogramas.
        /// </summary>
        public decimal? Weight { get; set; }

        /// <summary>
        /// Altura do usuário em metros.
        /// </summary>
        public decimal? Height { get; set; }

        /// <summary>
        /// Data de nascimento do usuário.
        /// </summary>
        public DateTime? BirthDate { get; set; }
    }
}
