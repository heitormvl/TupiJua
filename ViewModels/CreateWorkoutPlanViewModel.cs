using System.ComponentModel.DataAnnotations;

namespace TupiJua.ViewModels
{
    /// <summary>
    /// ViewModel para criar um plano de treino.
    /// </summary>
    public class CreateWorkoutPlanViewModel
    {
        /// <summary>
        /// ID do exercício.
        /// </summary>
        [Required(ErrorMessage = "Dê um nome ao seu plano (ex: Treino A).")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Exercícios associados ao plano de treino.
        /// </summary>
        public List<CreateWorkoutPlanExerciseViewModel> Exercises { get; set; } = new();
    }
}
