using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TupiJua.Models;
using TupiJua.ViewModels;

namespace TupiJua.Controllers
{
    [Authorize]
    public class WorkoutPlanController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public WorkoutPlanController(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Lista os planos de treino do usuário logado
        /// </summary>
        /// <returns>Uma view com a lista de planos de treino</returns>
        [HttpGet]
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var workoutPlans = _context.WorkoutPlans
                .Where(wp => wp.UserId == userId)
                .Include(wp => wp.PlanExercises)
                .ThenInclude(pe => pe.Exercise)
                .ToList();

            return View(workoutPlans);
        }

        /// <summary>
        /// Exibe o formulário para criação de um novo plano de treino
        /// </summary>
        /// <returns>Uma view com o formulário de criação</returns>
        [HttpGet]
        public IActionResult Create()
        {
            var exercises = _context.Exercises
                .Include(e => e.ExerciseMuscleGroups)
                .ThenInclude(emg => emg.MuscleGroup)
                .OrderBy(e => e.Name)
                .ToList();

            ViewBag.Exercises = exercises;
            return View();
        }

        /// <summary>
        /// Processa o formulário de criação de um novo plano de treino
        /// </summary>
        /// <param name="model">O modelo contendo os dados do novo plano de treino</param>
        /// <returns>Uma redireção para a lista de planos de treino</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateWorkoutPlanViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);

                var workoutPlan = new WorkoutPlan
                {
                    UserId = userId!,
                    Name = model.Name,
                    IsActive = true,
                    PlanExercises = model.Exercises.Select(e => new WorkoutPlanExercise
                    {
                        ExerciseId = e.ExerciseId,
                        TargetSets = e.TargetSets,
                        TargetReps = e.TargetReps!,
                        RecommendedRestTime = e.RecommendedRestTime,
                        RestInMinutes = e.RestInMinutes,
                        Order = e.Order
                    }).ToList()
                };

                _context.WorkoutPlans.Add(workoutPlan);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            var exercises = _context.Exercises
                .Include(e => e.ExerciseMuscleGroups)
                .ThenInclude(emg => emg.MuscleGroup)
                .OrderBy(e => e.Name)
                .ToList();
            ViewBag.Exercises = exercises;
            return View(model);
        }

        /// <summary>
        /// Exibe o formulário para edição de um plano de treino existente
        /// </summary>
        /// <param name="id">O ID do plano de treino a ser editado</param>
        /// <returns>Uma view com o formulário de edição</returns>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var workoutPlan = await _context.WorkoutPlans
                .Include(wp => wp.PlanExercises)
                .FirstOrDefaultAsync(wp => wp.Id == id && wp.UserId == userId);

            if (workoutPlan == null)
            {
                return NotFound();
            }

            var model = new EditWorkoutPlanViewModel
            {
                Name = workoutPlan.Name,
                Exercises = workoutPlan.PlanExercises.Select(pe => new EditWorkoutPlanExerciseViewModel
                {
                    Id = pe.Id,
                    ExerciseId = pe.ExerciseId,
                    TargetSets = pe.TargetSets,
                    TargetReps = pe.TargetReps,
                    RecommendedRestTime = pe.RecommendedRestTime,
                    RestInMinutes = pe.RestInMinutes,
                    Order = pe.Order
                }).ToList()
            };

            var exercises = await _context.Exercises
                .Include(e => e.ExerciseMuscleGroups)
                .ThenInclude(emg => emg.MuscleGroup)
                .OrderBy(e => e.Name)
                .ToListAsync();
            ViewBag.Exercises = exercises;

            return View(model);
        }

        /// <summary>
        /// Processa o formulário de edição de um plano de treino existente
        /// </summary>
        /// <param name="id">O ID do plano de treino a ser editado</param>
        /// <param name="model">O modelo contendo os dados atualizados do plano de treino</param>
        /// <returns>Uma redireção para a lista de planos de treino</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditWorkoutPlanViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var workoutPlan = await _context.WorkoutPlans
                    .Include(wp => wp.PlanExercises)
                    .FirstOrDefaultAsync(wp => wp.Id == id && wp.UserId == userId);

                if (workoutPlan == null) return NotFound();

                workoutPlan.Name = model.Name;

                // --- LÓGICA DE REMOÇÃO ---
                // Pegamos os IDs que vieram da View
                var updatedExerciseIds = model.Exercises.Select(e => e.Id).ToList();

                // Identificamos o que está no banco mas não veio na lista nova
                var removedExercises = workoutPlan.PlanExercises
                    .Where(pe => !updatedExerciseIds.Contains(pe.Id))
                    .ToList();

                // Removemos do banco
                foreach (var removed in removedExercises)
                {
                    workoutPlan.PlanExercises.Remove(removed);
                }
                // -------------------------

                // Update existing exercises and add new ones
                foreach (var exerciseModel in model.Exercises)
                {
                    var existingExercise = workoutPlan.PlanExercises
                        .FirstOrDefault(pe => pe.Id == exerciseModel.Id);

                    if (existingExercise != null)
                    {
                        existingExercise.TargetSets = exerciseModel.TargetSets;
                        existingExercise.TargetReps = exerciseModel.TargetReps;
                        existingExercise.RecommendedRestTime = exerciseModel.RecommendedRestTime;
                        existingExercise.RestInMinutes = exerciseModel.RestInMinutes;
                        existingExercise.Order = exerciseModel.Order;
                        existingExercise.ExerciseId = exerciseModel.ExerciseId; // Caso mude o exercício
                    }
                    else
                    {
                        workoutPlan.PlanExercises.Add(new WorkoutPlanExercise
                        {
                            ExerciseId = exerciseModel.ExerciseId,
                            TargetSets = exerciseModel.TargetSets,
                            TargetReps = exerciseModel.TargetReps,
                            RecommendedRestTime = exerciseModel.RecommendedRestTime,
                            RestInMinutes = exerciseModel.RestInMinutes,
                            Order = exerciseModel.Order
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopular ViewBag em caso de erro de validação
            ViewBag.Exercises = await _context.Exercises
                .Include(e => e.ExerciseMuscleGroups)
                .ThenInclude(emg => emg.MuscleGroup)
                .OrderBy(e => e.Name)
                .ToListAsync();

            return View(model);
        }

        /// <summary>
        /// Processa a exclusão de um plano de treino existente
        /// </summary>
        /// <param name="id">O ID do plano de treino a ser excluído</param>
        /// <returns>Uma redireção para a lista de planos de treino</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var workoutPlan = await _context.WorkoutPlans
                .FirstOrDefaultAsync(wp => wp.Id == id && wp.UserId == userId);

            if (workoutPlan == null)
            {
                return NotFound();
            }

            _context.WorkoutPlans.Remove(workoutPlan);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
