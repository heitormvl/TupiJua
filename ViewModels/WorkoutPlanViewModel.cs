namespace TupiJua.ViewModels
{
    /// <summary>
    /// Modelo de visualização para um plano de treino.
    /// </summary>
    public class WorkoutPlanViewModel
    {
        /// <summary>
        /// Identificador do plano de treino.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do plano de treino.
        /// </summary>
        public string Name { get; set; } = null!;
    }
}
