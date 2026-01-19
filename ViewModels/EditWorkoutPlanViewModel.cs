using System.ComponentModel.DataAnnotations;

namespace TupiJua.ViewModels
{
    /// <summary>
    /// ViewModel para editar um plano de treino.
    /// </summary>
    public class EditWorkoutPlanViewModel
    {
        /// <summary>
        /// ID do plano de treino.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID do exercício.
        /// </summary>
        [Required(ErrorMessage = "Dê um nome ao seu plano (ex: Treino A).")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Exercícios associados ao plano de treino.
        /// </summary>
        public List<EditWorkoutPlanExerciseViewModel> Exercises { get; set; } = new();
    }
}
