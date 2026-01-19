using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TupiJua.Models;
using TupiJua.ViewModels;

namespace TupiJua.Controllers
{
    public class TrainingController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public TrainingController(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Exibe a lista de sessões de treino do usuário e planos de treino ativos.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var sessions = await _context.WorkoutSessions
                .Where(ws => ws.UserId == userId)
                .Include(ws => ws.LoggedExercises)
                .ThenInclude(le => le.Exercise)
                .OrderByDescending(ws => ws.Date)
                .ToListAsync();
            var WorkoutPlans = await _context.WorkoutPlans
                .Where(wp => wp.UserId == userId && wp.IsActive)
                .ToListAsync();
            var workoutPlansModels = new List<WorkoutPlanViewModel>();
            foreach (var plan in WorkoutPlans)
            {
                workoutPlansModels.Add(new WorkoutPlanViewModel
                {
                    Id = plan.Id,
                    Name = plan.Name
                });
            }
            return View((sessions, workoutPlansModels));
        }

        /// <summary>
        /// Inicia uma nova sessão de treino livre para o usuário.
        /// </summary>
        /// <returns>Redireciona para a página de adicionar exercício na nova sessão</returns>
        [HttpPost]
        public async Task<IActionResult> StartFreeTraining()
        {
            var userId = _userManager.GetUserId(User);
            var session = new WorkoutSession
            {
                UserId = userId!
            };

            _context.WorkoutSessions.Add(session);
            await _context.SaveChangesAsync();

            return RedirectToAction("AddExercise", new { sessionId = session.Id });
        }
        
        /// <summary>
        /// Exibe o formulário para adicionar um exercício a uma sessão de treino.
        /// </summary>
        /// <param name="sessionId">ID da sessão de treino</param>
        /// <returns>View para adicionar exercício</returns>
        [HttpGet]
        public async Task<IActionResult> AddExercise(int sessionId)
        {
            ViewBag.Exercises = await _context.Exercises.OrderBy(e => e.Name).ToListAsync();

            var model = new LogExerciseViewModel
            {
                WorkoutSessionId = sessionId
            };
            return View(model);
        }

        /// <summary>
        /// Obtém o último exercício registrado pelo usuário para um exercício específico.
        /// </summary>
        /// <param name="exerciseId">ID do exercício</param>
        /// <returns>Detalhes do último exercício registrado em formato JSON</returns>
        [HttpGet]
        public async Task<IActionResult> GetLastLoggedExercise(int exerciseId)
        {
            var userId = _userManager.GetUserId(User);
            var lastLoggedExercise = await _context.LoggedExercises
                .Where(le => le.ExerciseId == exerciseId && le.WorkoutSession.UserId == userId)
                .OrderByDescending(le => le.WorkoutSession.Date)
                .FirstOrDefaultAsync();

            if (lastLoggedExercise != null)
            {
                var model = new LastExerciseViewModel
                {
                    Sets = lastLoggedExercise.Sets,
                    Reps = lastLoggedExercise.Reps,
                    Weight = lastLoggedExercise.Weight,
                    RestSeconds = lastLoggedExercise.RestSeconds,
                    Observation = lastLoggedExercise.Observation,
                    ShouldIncreaseLoad = true
                };
                return Json(model);
            }

            return Json(null);
        }

        /// <summary>
        /// Processa o formulário de adição de exercício a uma sessão de treino.
        /// </summary>
        /// <param name="model">Modelo de visualização contendo os dados do exercício</param>
        /// <param name="action">Ação do botão pressionado ("continue" ou "finish")</param>
        /// <returns>Redireciona para a próxima ação apropriada</returns>
        [HttpPost]
        public async Task<IActionResult> AddExercise(LogExerciseViewModel model, string action)
        {
            if (ModelState.IsValid)
            {
                var loggedExercise = new LoggedExercise
                {
                    WorkoutSessionId = model.WorkoutSessionId,
                    ExerciseId = model.ExerciseId,
                    Sets = model.Sets,
                    Reps = model.Reps,
                    IntegerReps = CalculateIntegerReps(model.Reps),
                    Weight = model.Weight,
                    RestSeconds = model.RestSeconds,
                    Observation = model.Observation
                };

                _context.LoggedExercises.Add(loggedExercise);
                await _context.SaveChangesAsync();

                // Check action parameter to decide where to redirect
                if (action == "finish")
                {
                    return RedirectToAction("Index");
                }

                return RedirectToAction("AddExercise", new { sessionId = model.WorkoutSessionId });
            }

            ViewBag.Exercises = await _context.Exercises.OrderBy(e => e.Name).ToListAsync();
            return View(model);
        }

        /// <summary>
        /// Calcula o valor inteiro de repetições a partir do formato de string.
        /// </summary>
        /// <param name="reps">String de repetições (pode ser fixa ou intervalo)</param>
        /// <returns>Valor inteiro representando as repetições</returns>
        private int CalculateIntegerReps(string reps)
        {
            if (reps.Contains('-'))
            {
                var parts = reps.Split('-');
                if (int.TryParse(parts[1].Trim(), out int upper))
                {
                    return upper;
                }
            }
            else
            {
                if (int.TryParse(reps.Trim(), out int fixedReps))
                {
                    return fixedReps;
                }
            }
            throw new ArgumentException("Format de repetições inválido.");
        }
    }
}
