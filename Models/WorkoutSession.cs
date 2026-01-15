using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TupiJua.Models
{
    /// <summary>
    /// Sessão de treino do usuário.
    /// </summary>
    public class WorkoutSession
    {
        /// <summary>
        /// Identificador da sessão de treino.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Identificador do usuário (chave estrangeira para AspNetUsers.Id).
        /// </summary>
        [Required]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Usuário dono da sessão.
        /// </summary>
        public virtual User? User { get; set; }

        /// <summary>
        /// Identificador do plano de treino (nulo se for Treino Livre).
        /// </summary>
        [ForeignKey(nameof(WorkoutPlan))]
        public int? WorkoutPlanId { get; set; }

        /// <summary>
        /// Plano de treino associado (pode ser nulo para Treino Livre).
        /// </summary>
        public virtual WorkoutPlan? WorkoutPlan { get; set; }

        /// <summary>
        /// Data e hora da sessão.
        /// </summary>
        [Column(TypeName = "datetime")]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; } = DateTime.Now;

        /// <summary>
        /// Duração total da sessão em minutos.
        /// </summary>
        public int? TotalDurationMinutes { get; set; }

        /// <summary>
        /// Exercícios registrados na sessão.
        /// </summary>
        public virtual ICollection<LoggedExercise> LoggedExercises { get; set; } = new List<LoggedExercise>();
    }
}