using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TupiJua.Models
{
    /// <summary>
    /// Usuário do sistema.
    /// </summary>
    public class User : IdentityUser
    {
        /// <summary>
        /// Peso do usuário em kg.
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal? Weight { get; set; }

        /// <summary>
        /// Altura do usuário em metros.
        /// </summary>
        [Column(TypeName = "decimal(4,2)")]
        public decimal? Height { get; set; }

        /// <summary>
        /// Data de nascimento do usuário.
        /// </summary>
        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        /// <summary>
        /// Planos de treino do usuário.
        /// </summary>
        public virtual ICollection<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();

        /// <summary>
        /// Sessões de treino do usuário.
        /// </summary>
        public virtual ICollection<WorkoutSession> WorkoutSessions { get; set; } = new List<WorkoutSession>();
    }
}