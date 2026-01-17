using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TupiJua.ViewModels
{
    /// <summary>
    /// ViewModel para edição de informações do usuário.
    /// </summary>
    public class EditUserViewModel
    {
        /// <summary>
        /// Peso do usuário em quilogramas.
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        [Range(0, 500, ErrorMessage = "Peso deve estar entre {1} e {2}.")]
        public decimal? Weight { get; set; }

        /// <summary>
        /// Altura do usuário em metros.
        /// </summary>
        [Column(TypeName = "decimal(4,2)")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        [Range(0, 3, ErrorMessage = "Altura deve estar entre {1} e {2}.")]
        public decimal? Height { get; set; }

        /// <summary>
        /// Data de nascimento do usuário.
        /// </summary>
        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
    }
}
