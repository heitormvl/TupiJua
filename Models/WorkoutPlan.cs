using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TupiJua.Models
{
    /// <summary>
    /// Plano de treino do usuário.
    /// </summary>
    public class WorkoutPlan
    {
        /// <summary>
        /// Identificador do plano de treino.
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
        /// Usuário dono do plano de treino.
        /// </summary>
        public virtual User? User { get; set; }

        /// <summary>
        /// Nome do plano (Ex: Treino A - Superiores).
        /// </summary>
        [Required]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Data e hora de criação do plano.
        /// </summary>
        [Column(TypeName = "datetime")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Sessões de treino associadas ao plano.
        /// </summary>
        public virtual ICollection<WorkoutSession> WorkoutSessions { get; set; } = new List<WorkoutSession>();
    }
}