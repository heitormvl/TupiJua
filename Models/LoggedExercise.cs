using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TupiJua.Models
{
    /// <summary>
    /// Registro de execução de um exercício em uma sessão de treino.
    /// </summary>
    public class LoggedExercise
    {
        /// <summary>
        /// Identificador do registro.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Identificador da sessão de treino associada.
        /// </summary>
        [Required]
        public int WorkoutSessionId { get; set; }

        /// <summary>
        /// Sessão de treino associada.
        /// </summary>
        [ForeignKey(nameof(WorkoutSessionId))]
        public WorkoutSession WorkoutSession { get; set; } = null!;

        /// <summary>
        /// Identificador do exercício registrado.
        /// </summary>
        [Required]
        public int ExerciseId { get; set; }

        /// <summary>
        /// Exercício associado.
        /// </summary>
        [ForeignKey(nameof(ExerciseId))]
        public Exercise Exercise { get; set; } = null!;

        /// <summary>
        /// Quantidade de séries totais.
        /// </summary>
        [Required]
        [Range(1, 100, ErrorMessage = "As séries devem ser entre {1} e {2}.")]
        public int Sets { get; set; }

        /// <summary>
        /// Repetições: pode ser fixo (ex.: "12") ou faixa (ex.: "10-12").
        /// </summary>
        [Required]
        [MaxLength(20)]
        [RegularExpression(@"^\d+(\s*-\s*\d+)?$", ErrorMessage = "Deve ser um número (ex.: '12') ou faixa (ex.: '10-12').")]
        public string Reps { get; set; } = null!;

        /// <summary>
        /// "Repetições inteiras para facilitar buscas e análises (ex.: 12 para "12" ou 13 para "10-15")."
        /// </summary>
        [Required]
        public int IntegerReps { get; set; }

        /// <summary>
        /// Carga utilizada (precision: 5,2).
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 999.99, ErrorMessage = "A carga deve ser entre {1} e {2}.")]
        public decimal Weight { get; set; }

        /// <summary>
        /// Tempo de descanso entre séries, em segundos.
        /// </summary>
        [Range(0, 3600, ErrorMessage = "O descanso deve ser entre {1} e {2} segundos.")]
        public int RestSeconds { get; set; }

        /// <summary>
        /// Notas rápidas sobre a execução.
        /// </summary>
        [MaxLength(1000)]
        public string? Observation { get; set; }

        /// <summary>
        /// Flag indicando se deve aumentar a carga no próximo treino.
        /// </summary>
        public bool ShouldIncreaseLoad { get; set; } = false;
    }
}