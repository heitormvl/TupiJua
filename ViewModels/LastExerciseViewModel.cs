namespace TupiJua.ViewModels
{
    /// <summary>
    /// Modelo de visualização para o último exercício registrado pelo usuário.
    /// </summary>
    public class LastExerciseViewModel
    {
        /// <summary>
        /// Número de séries do exercício.
        /// </summary>
        public int Sets { get; set; }

        /// <summary>
        /// Repetições do exercício em formato de string (pode ser fixa ou intervalo).
        /// </summary>
        public string Reps { get; set; } = null!;

        /// <summary>
        /// Carga utilizada no exercício.
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// Tempo de descanso entre as séries.
        /// </summary>
        public int RestTime { get; set; }

        /// <summary>
        /// Flag indicando se o tempo de descanso está em minutos (true) ou segundos (false).
        /// </summary>
        public bool RestInMinutes { get; set; } = false;

        /// <summary>
        /// Observações adicionais sobre o exercício.
        /// </summary>
        public string? Observation { get; set; }

        /// <summary>
        /// Indica se a carga deve ser aumentada no próximo exercício.
        /// </summary>
        public bool ShouldIncreaseLoad { get; set; } = false;
    }
}
