using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TupiJua.Models;
using TupiJua.ViewModels;

namespace TupiJua.Controllers
{
    [Authorize]
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
                .Include(wp => wp.PlanExercises)
                .ToListAsync();
            var workoutPlansModels = new List<WorkoutPlanViewModel>();
            foreach (var plan in WorkoutPlans)
            {
                workoutPlansModels.Add(new WorkoutPlanViewModel
                {
                    Id = plan.Id,
                    Name = plan.Name,
                    ExerciseCount = plan.PlanExercises.Count
                });
            }
            return View((sessions, workoutPlansModels));
        }

        /// <summary>
        /// Exibe os detalhes de uma sessão de treino.
        /// </summary>
        /// <param name="sessionId">ID da sessão de treino</param>
        /// <returns>View com os detalhes da sessão</returns>
        [HttpGet]
        public async Task<IActionResult> ViewWorkout(int sessionId)
        {
            var userId = _userManager.GetUserId(User);
            var session = await _context.WorkoutSessions
                .Include(ws => ws.WorkoutPlan)
                .Include(ws => ws.LoggedExercises)
                .ThenInclude(le => le.Exercise)
                .ThenInclude(e => e.ExerciseMuscleGroups)
                .ThenInclude(emg => emg.MuscleGroup)
                .FirstOrDefaultAsync(ws => ws.Id == sessionId && ws.UserId == userId);

            if (session == null)
            {
                return RedirectToAction("Index");
            }

            return View(session);
        }

        /// <summary>
        /// Verifica se o usuário já tem um treino registrado no dia de hoje.
        /// </summary>
        /// <returns>JSON com informação sobre treinos do dia</returns>
        [HttpGet]
        public async Task<IActionResult> CheckTodayWorkout()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Json(new { hasWorkout = false });
            }

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var hasWorkout = await _context.WorkoutSessions
                .AnyAsync(ws => ws.UserId == userId && ws.Date >= today && ws.Date < tomorrow);

            return Json(new { hasWorkout });
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
            ViewBag.Exercises = await _context.Exercises
                .Include(e => e.ExerciseMuscleGroups)
                .ThenInclude(emg => emg.MuscleGroup)
                .OrderBy(e => e.Name)
                .ToListAsync();

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
                    RestTime = lastLoggedExercise.RestTime,
                    RestInMinutes = lastLoggedExercise.RestInMinutes,
                    Observation = lastLoggedExercise.Observation,
                    ShouldIncreaseLoad = lastLoggedExercise.ShouldIncreaseLoad
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
                    RestTime = model.RestTime,
                    RestInMinutes = model.RestInMinutes,
                    Observation = model.Observation,
                    ShouldIncreaseLoad = model.ShouldIncreaseLoad
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

            ViewBag.Exercises = await _context.Exercises
                .Include(e => e.ExerciseMuscleGroups)
                .ThenInclude(emg => emg.MuscleGroup)
                .OrderBy(e => e.Name)
                .ToListAsync();
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

        [HttpPost]
        public async Task<IActionResult> StartTrainingFromPlan(int workoutPlanId)
        {
            var userId = _userManager.GetUserId(User);

            // 1. Criar a sessão vinculada ao plano
            var session = new WorkoutSession
            {
                UserId = userId!,
                WorkoutPlanId = workoutPlanId
            };

            _context.WorkoutSessions.Add(session);
            await _context.SaveChangesAsync();

            // 2. Redirecionar para a execução guiada
            return RedirectToAction("ExecutePlan", new { sessionId = session.Id });
        }

        /// <summary>
        /// Exibe a tela de execução guiada do plano de treino.
        /// </summary>
        /// <param name="sessionId">ID da sessão de treino</param>
        /// <returns>View com a lista de exercícios do plano e status de execução</returns>
        [HttpGet]
        public async Task<IActionResult> ExecutePlan(int sessionId)
        {
            var session = await _context.WorkoutSessions
                .Include(ws => ws.WorkoutPlan)
                .ThenInclude(wp => wp!.PlanExercises)
                .ThenInclude(wpe => wpe.Exercise)
                .Include(ws => ws.LoggedExercises)
                .FirstOrDefaultAsync(ws => ws.Id == sessionId);

            if (session == null || session.WorkoutPlan == null)
            {
                return RedirectToAction("Index");
            }

            // Verificar se o usuário é o dono da sessão
            var userId = _userManager.GetUserId(User);
            if (session.UserId != userId)
            {
                return RedirectToAction("Index");
            }

            // Montar o ViewModel
            var model = new TrainingFromPlanViewModel
            {
                SessionId = session.Id,
                WorkoutPlanId = session.WorkoutPlan.Id,
                PlanName = session.WorkoutPlan.Name,
                Exercises = session.WorkoutPlan.PlanExercises
                    .OrderBy(wpe => wpe.Order)
                    .Select(wpe => new PlanExerciseItemViewModel
                    {
                        ExerciseId = wpe.ExerciseId,
                        ExerciseName = wpe.Exercise?.Name ?? "",
                        Order = wpe.Order,
                        TargetSets = wpe.TargetSets,
                        TargetReps = wpe.TargetReps,
                        RecommendedRestTime = wpe.RecommendedRestTime,
                        RestInMinutes = wpe.RestInMinutes,
                        IsCompleted = session.LoggedExercises.Any(le => le.ExerciseId == wpe.ExerciseId && !le.IsSkipped),
                        IsSkipped = session.LoggedExercises.Any(le => le.ExerciseId == wpe.ExerciseId && le.IsSkipped)
                    })
                    .ToList()
            };

            return View(model);
        }

        /// <summary>
        /// Exibe o formulário para adicionar um exercício específico de um plano.
        /// </summary>
        /// <param name="sessionId">ID da sessão de treino</param>
        /// <param name="exerciseId">ID do exercício a ser registrado</param>
        /// <returns>View para adicionar exercício com dados pré-preenchidos</returns>
        [HttpGet]
        public async Task<IActionResult> AddExerciseFromPlan(int sessionId, int exerciseId)
        {
            var session = await _context.WorkoutSessions
                .Include(ws => ws.WorkoutPlan)
                .ThenInclude(wp => wp!.PlanExercises)
                .FirstOrDefaultAsync(ws => ws.Id == sessionId);

            if (session == null || session.WorkoutPlan == null)
            {
                return RedirectToAction("Index");
            }

            // Verificar se o usuário é o dono da sessão
            var userId = _userManager.GetUserId(User);
            if (session.UserId != userId)
            {
                return RedirectToAction("Index");
            }

            // Buscar informações do exercício no plano
            var planExercise = session.WorkoutPlan.PlanExercises
                .FirstOrDefault(wpe => wpe.ExerciseId == exerciseId);

            if (planExercise == null)
            {
                return RedirectToAction("ExecutePlan", new { sessionId });
            }

            // Buscar última execução do exercício
            var lastLoggedExercise = await _context.LoggedExercises
                .Where(le => le.ExerciseId == exerciseId && le.WorkoutSession.UserId == userId && !le.IsSkipped)
                .OrderByDescending(le => le.WorkoutSession.Date)
                .FirstOrDefaultAsync();

            var exercise = await _context.Exercises.FindAsync(exerciseId);
            ViewBag.Exercise = exercise;
            ViewBag.TargetSets = planExercise.TargetSets;
            ViewBag.TargetReps = planExercise.TargetReps;
            ViewBag.RecommendedRestTime = planExercise.RecommendedRestTime;
            ViewBag.RestInMinutes = planExercise.RestInMinutes;
            ViewBag.FromPlan = true;

            // Passar dados da última execução se existir
            if (lastLoggedExercise != null)
            {
                ViewBag.LastSets = lastLoggedExercise.Sets;
                ViewBag.LastReps = lastLoggedExercise.Reps;
                ViewBag.LastWeight = lastLoggedExercise.Weight;
                ViewBag.LastRestTime = lastLoggedExercise.RestTime;
                ViewBag.LastRestInMinutes = lastLoggedExercise.RestInMinutes;
                ViewBag.HasLastExecution = true;
            }
            else
            {
                ViewBag.HasLastExecution = false;
            }

            var model = new LogExerciseViewModel
            {
                WorkoutSessionId = sessionId,
                ExerciseId = exerciseId,
                Sets = planExercise.TargetSets,
                Reps = planExercise.TargetReps ?? "12",
                RestTime = planExercise.RecommendedRestTime > 0 ? planExercise.RecommendedRestTime : 60,
                RestInMinutes = planExercise.RestInMinutes
            };

            return View("AddExercise", model);
        }

        /// <summary>
        /// Processa o formulário de adição de exercício de um plano.
        /// </summary>
        /// <param name="model">Modelo de visualização contendo os dados do exercício</param>
        /// <param name="action">Ação do botão pressionado ("continue" ou "finish")</param>
        /// <returns>Redireciona para ExecutePlan</returns>
        [HttpPost]
        public async Task<IActionResult> AddExerciseFromPlan(LogExerciseViewModel model, string action)
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
                    RestTime = model.RestTime,
                    RestInMinutes = model.RestInMinutes,
                    Observation = model.Observation,
                    ShouldIncreaseLoad = model.ShouldIncreaseLoad
                };

                _context.LoggedExercises.Add(loggedExercise);
                await _context.SaveChangesAsync();

                // Sempre volta para ExecutePlan
                return RedirectToAction("ExecutePlan", new { sessionId = model.WorkoutSessionId });
            }

            var session = await _context.WorkoutSessions
                .Include(ws => ws.WorkoutPlan)
                .ThenInclude(wp => wp!.PlanExercises)
                .FirstOrDefaultAsync(ws => ws.Id == model.WorkoutSessionId);

            if (session?.WorkoutPlan != null)
            {
                var planExercise = session.WorkoutPlan.PlanExercises
                    .FirstOrDefault(wpe => wpe.ExerciseId == model.ExerciseId);

                if (planExercise != null)
                {
                    var exercise = await _context.Exercises.FindAsync(model.ExerciseId);
                    ViewBag.Exercise = exercise;
                    ViewBag.TargetSets = planExercise.TargetSets;
                    ViewBag.TargetReps = planExercise.TargetReps;
                    ViewBag.RecommendedRestTime = planExercise.RecommendedRestTime;
                    ViewBag.RestInMinutes = planExercise.RestInMinutes;
                    ViewBag.FromPlan = true;
                }
            }

            return View("AddExercise", model);
        }

        /// <summary>
        /// Marca um exercício como pulado em uma sessão de plano de treino.
        /// </summary>
        /// <param name="sessionId">ID da sessão de treino</param>
        /// <param name="exerciseId">ID do exercício a ser pulado</param>
        /// <returns>Redireciona para ExecutePlan</returns>
        [HttpPost]
        public async Task<IActionResult> SkipExerciseFromPlan(int sessionId, int exerciseId)
        {
            var session = await _context.WorkoutSessions
                .Include(ws => ws.WorkoutPlan)
                .ThenInclude(wp => wp!.PlanExercises)
                .Include(ws => ws.LoggedExercises)
                .FirstOrDefaultAsync(ws => ws.Id == sessionId);

            if (session == null || session.WorkoutPlan == null)
            {
                return RedirectToAction("Index");
            }

            // Verificar se o usuário é o dono da sessão
            var userId = _userManager.GetUserId(User);
            if (session.UserId != userId)
            {
                return RedirectToAction("Index");
            }

            // Verificar se o exercício pertence ao plano
            var planExercise = session.WorkoutPlan.PlanExercises
                .FirstOrDefault(wpe => wpe.ExerciseId == exerciseId);

            if (planExercise == null)
            {
                return RedirectToAction("ExecutePlan", new { sessionId });
            }

            // Verificar se o exercício já foi registrado (completado ou pulado)
            var existingLog = session.LoggedExercises
                .FirstOrDefault(le => le.ExerciseId == exerciseId);

            if (existingLog != null)
            {
                // Se já existe e não está pulado, não permitir pular exercício já completado
                if (!existingLog.IsSkipped)
                {
                    return RedirectToAction("ExecutePlan", new { sessionId });
                }
                // Se já está pulado, não fazer nada
            }
            else
            {
                // Criar novo registro marcado como pulado
                var skippedExercise = new LoggedExercise
                {
                    WorkoutSessionId = sessionId,
                    ExerciseId = exerciseId,
                    Sets = 0,
                    Reps = "0",
                    IntegerReps = 0,
                    Weight = 0,
                    RestTime = 0,
                    IsSkipped = true
                };

                _context.LoggedExercises.Add(skippedExercise);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("ExecutePlan", new { sessionId });
        }

        /// <summary>
        /// Finaliza uma sessão de treino marcando-a como concluída.
        /// </summary>
        /// <param name="sessionId">ID da sessão de treino</param>
        /// <returns>Redireciona para a visualização do treino concluído</returns>
        [HttpPost]
        public async Task<IActionResult> FinishWorkout(int sessionId)
        {
            var userId = _userManager.GetUserId(User);
            var session = await _context.WorkoutSessions
                .FirstOrDefaultAsync(ws => ws.Id == sessionId && ws.UserId == userId);

            if (session == null)
            {
                return RedirectToAction("Index");
            }

            session.IsCompleted = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("ViewWorkout", new { sessionId });
        }

        /// <summary>
        /// Exibe o formulário para editar um exercício já registrado.
        /// </summary>
        /// <param name="id">ID do LoggedExercise</param>
        /// <returns>View para editar exercício</returns>
        [HttpGet]
        public async Task<IActionResult> EditLoggedExercise(int id)
        {
            var userId = _userManager.GetUserId(User);
            var loggedExercise = await _context.LoggedExercises
                .Include(le => le.WorkoutSession)
                .Include(le => le.Exercise)
                .FirstOrDefaultAsync(le => le.Id == id && le.WorkoutSession.UserId == userId);

            if (loggedExercise == null)
            {
                return RedirectToAction("Index");
            }

            // Não permitir edição de exercícios em treinos concluídos
            if (loggedExercise.WorkoutSession.IsCompleted)
            {
                return RedirectToAction("ViewWorkout", new { sessionId = loggedExercise.WorkoutSessionId });
            }

            var model = new EditLoggedExerciseViewModel
            {
                Id = loggedExercise.Id,
                WorkoutSessionId = loggedExercise.WorkoutSessionId,
                ExerciseId = loggedExercise.ExerciseId,
                Sets = loggedExercise.Sets,
                Reps = loggedExercise.Reps,
                Weight = loggedExercise.Weight,
                RestTime = loggedExercise.RestTime,
                RestInMinutes = loggedExercise.RestInMinutes,
                Observation = loggedExercise.Observation,
                ShouldIncreaseLoad = loggedExercise.ShouldIncreaseLoad
            };

            ViewBag.Exercise = loggedExercise.Exercise;
            return View(model);
        }

        /// <summary>
        /// Processa o formulário de edição de um exercício já registrado.
        /// </summary>
        /// <param name="model">Modelo de visualização contendo os dados do exercício</param>
        /// <returns>Redireciona para ViewWorkout</returns>
        [HttpPost]
        public async Task<IActionResult> EditLoggedExercise(EditLoggedExerciseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var loggedExercise = await _context.LoggedExercises
                    .Include(le => le.WorkoutSession)
                    .FirstOrDefaultAsync(le => le.Id == model.Id && le.WorkoutSession.UserId == userId);

                if (loggedExercise == null)
                {
                    return RedirectToAction("Index");
                }

                // Não permitir edição de exercícios em treinos concluídos
                if (loggedExercise.WorkoutSession.IsCompleted)
                {
                    return RedirectToAction("ViewWorkout", new { sessionId = model.WorkoutSessionId });
                }

                loggedExercise.Sets = model.Sets;
                loggedExercise.Reps = model.Reps;
                loggedExercise.IntegerReps = CalculateIntegerReps(model.Reps);
                loggedExercise.Weight = model.Weight;
                loggedExercise.RestTime = model.RestTime;
                loggedExercise.RestInMinutes = model.RestInMinutes;
                loggedExercise.Observation = model.Observation;
                loggedExercise.ShouldIncreaseLoad = model.ShouldIncreaseLoad;

                await _context.SaveChangesAsync();

                return RedirectToAction("ViewWorkout", new { sessionId = model.WorkoutSessionId });
            }

            var exercise = await _context.Exercises.FindAsync(model.ExerciseId);
            ViewBag.Exercise = exercise;
            return View(model);
        }

        /// <summary>
        /// Exclui um exercício registrado de uma sessão de treino.
        /// </summary>
        /// <param name="id">ID do LoggedExercise</param>
        /// <returns>Redireciona para ViewWorkout</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteLoggedExercise(int id)
        {
            var userId = _userManager.GetUserId(User);
            var loggedExercise = await _context.LoggedExercises
                .Include(le => le.WorkoutSession)
                .FirstOrDefaultAsync(le => le.Id == id && le.WorkoutSession.UserId == userId);

            if (loggedExercise == null)
            {
                return RedirectToAction("Index");
            }

            // Não permitir exclusão de exercícios em treinos concluídos
            if (loggedExercise.WorkoutSession.IsCompleted)
            {
                return RedirectToAction("ViewWorkout", new { sessionId = loggedExercise.WorkoutSessionId });
            }

            var sessionId = loggedExercise.WorkoutSessionId;
            _context.LoggedExercises.Remove(loggedExercise);
            await _context.SaveChangesAsync();

            return RedirectToAction("ViewWorkout", new { sessionId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWorkoutSession(int sessionId)
        {
            var userId = _userManager.GetUserId(User);
            var session = await _context.WorkoutSessions
                .FirstOrDefaultAsync(ws => ws.Id == sessionId && ws.UserId == userId);

            if (session == null)
            {
                return RedirectToAction("Index");
            }

            _context.WorkoutSessions.Remove(session);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
