using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TupiJua.Models;
using TupiJua.ViewModels;

namespace TupiJua.Controllers
{
    [Authorize (Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AdminController(AppDbContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Exibe a página principal de administração
        /// </summary>
        /// <returns>View com estatísticas e acesso às funcionalidades</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var muscleGroupCount = await _context.MuscleGroups.CountAsync();
            var exerciseCount = await _context.Exercises.CountAsync();
            
            return View((muscleGroupCount, exerciseCount));
        }

        #region MuscleGroup Management

        /// <summary>
        /// Lista todos os grupos musculares
        /// </summary>
        /// <returns>View com lista de grupos musculares</returns>
        [HttpGet]
        public async Task<IActionResult> MuscleGroups()
        {
            var muscleGroups = await _context.MuscleGroups
                .Include(mg => mg.ExerciseMuscleGroups)
                .OrderBy(mg => mg.Name)
                .ToListAsync();

            return View(muscleGroups);
        }

        /// <summary>
        /// Exibe o formulário para criar um novo grupo muscular
        /// </summary>
        /// <returns>View com formulário de criação</returns>
        [HttpGet]
        public IActionResult CreateMuscleGroup()
        {
            return View();
        }

        /// <summary>
        /// Processa a criação de um novo grupo muscular
        /// </summary>
        /// <param name="model">Dados do grupo muscular</param>
        /// <returns>Redirecionamento para lista de grupos musculares</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMuscleGroup(MuscleGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                var muscleGroup = new MuscleGroup
                {
                    Name = model.Name,
                    Description = model.Description,
                    Color = model.Color
                };

                _context.MuscleGroups.Add(muscleGroup);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MuscleGroups));
            }

            return View(model);
        }

        /// <summary>
        /// Exibe o formulário para editar um grupo muscular
        /// </summary>
        /// <param name="id">ID do grupo muscular</param>
        /// <returns>View com formulário de edição</returns>
        [HttpGet]
        public async Task<IActionResult> EditMuscleGroup(int id)
        {
            var muscleGroup = await _context.MuscleGroups.FindAsync(id);

            if (muscleGroup == null)
            {
                return NotFound();
            }

            var model = new MuscleGroupViewModel
            {
                Id = muscleGroup.Id,
                Name = muscleGroup.Name,
                Description = muscleGroup.Description,
                Color = muscleGroup.Color
            };

            return View(model);
        }

        /// <summary>
        /// Processa a edição de um grupo muscular
        /// </summary>
        /// <param name="id">ID do grupo muscular</param>
        /// <param name="model">Dados atualizados do grupo muscular</param>
        /// <returns>Redirecionamento para lista de grupos musculares</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMuscleGroup(int id, MuscleGroupViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var muscleGroup = await _context.MuscleGroups.FindAsync(id);

                if (muscleGroup == null)
                {
                    return NotFound();
                }

                muscleGroup.Name = model.Name;
                muscleGroup.Description = model.Description;
                muscleGroup.Color = model.Color;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MuscleGroups));
            }

            return View(model);
        }

        /// <summary>
        /// Exclui um grupo muscular
        /// </summary>
        /// <param name="id">ID do grupo muscular</param>
        /// <returns>Redirecionamento para lista de grupos musculares</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMuscleGroup(int id)
        {
            var muscleGroup = await _context.MuscleGroups
                .Include(mg => mg.ExerciseMuscleGroups)
                .FirstOrDefaultAsync(mg => mg.Id == id);

            if (muscleGroup == null)
            {
                return NotFound();
            }

            // Verifica se há exercícios associados
            if (muscleGroup.ExerciseMuscleGroups.Any())
            {
                TempData["ErrorMessage"] = $"Não é possível excluir o grupo muscular '{muscleGroup.Name}' pois ele possui {muscleGroup.ExerciseMuscleGroups.Count} exercício(s) associado(s).";
                return RedirectToAction(nameof(MuscleGroups));
            }

            _context.MuscleGroups.Remove(muscleGroup);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Grupo muscular '{muscleGroup.Name}' excluído com sucesso!";
            return RedirectToAction(nameof(MuscleGroups));
        }

        #endregion

        #region Exercise Management

        /// <summary>
        /// Lista todos os exercícios
        /// </summary>
        /// <returns>View com lista de exercícios</returns>
        [HttpGet]
        public async Task<IActionResult> Exercises()
        {
            var exercises = await _context.Exercises
                .Include(e => e.ExerciseMuscleGroups)
                .ThenInclude(emg => emg.MuscleGroup)
                .OrderBy(e => e.Name)
                .ToListAsync();

            return View(exercises);
        }

        /// <summary>
        /// Exibe o formulário para criar um novo exercício
        /// </summary>
        /// <returns>View com formulário de criação</returns>
        [HttpGet]
        public async Task<IActionResult> CreateExercise()
        {
            ViewBag.MuscleGroups = await _context.MuscleGroups.OrderBy(mg => mg.Name).ToListAsync();
            return View();
        }

        /// <summary>
        /// Processa a criação de um novo exercício
        /// </summary>
        /// <param name="model">Dados do exercício</param>
        /// <returns>Redirecionamento para lista de exercícios</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExercise(ExerciseViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exercise = new Exercise
                {
                    Name = model.Name,
                    Description = model.Description
                };

                _context.Exercises.Add(exercise);
                await _context.SaveChangesAsync();

                // Adiciona associações com grupos musculares
                if (model.MuscleGroupIds != null && model.MuscleGroupIds.Any())
                {
                    foreach (var muscleGroupId in model.MuscleGroupIds)
                    {
                        var isPrimary = model.PrimaryMuscleGroupIds?.Contains(muscleGroupId) ?? false;
                        
                        var exerciseMuscleGroup = new ExerciseMuscleGroup
                        {
                            ExerciseId = exercise.Id,
                            MuscleGroupId = muscleGroupId,
                            IsPrimary = isPrimary
                        };

                        _context.ExerciseMuscleGroups.Add(exerciseMuscleGroup);
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Exercises));
            }

            ViewBag.MuscleGroups = await _context.MuscleGroups.OrderBy(mg => mg.Name).ToListAsync();
            return View(model);
        }

        /// <summary>
        /// Exibe o formulário para editar um exercício
        /// </summary>
        /// <param name="id">ID do exercício</param>
        /// <returns>View com formulário de edição</returns>
        [HttpGet]
        public async Task<IActionResult> EditExercise(int id)
        {
            var exercise = await _context.Exercises
                .Include(e => e.ExerciseMuscleGroups)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exercise == null)
            {
                return NotFound();
            }

            var model = new ExerciseViewModel
            {
                Id = exercise.Id,
                Name = exercise.Name,
                Description = exercise.Description,
                MuscleGroupIds = exercise.ExerciseMuscleGroups.Select(emg => emg.MuscleGroupId).ToList(),
                PrimaryMuscleGroupIds = exercise.ExerciseMuscleGroups.Where(emg => emg.IsPrimary).Select(emg => emg.MuscleGroupId).ToList()
            };

            ViewBag.MuscleGroups = await _context.MuscleGroups.OrderBy(mg => mg.Name).ToListAsync();
            return View(model);
        }

        /// <summary>
        /// Processa a edição de um exercício
        /// </summary>
        /// <param name="id">ID do exercício</param>
        /// <param name="model">Dados atualizados do exercício</param>
        /// <returns>Redirecionamento para lista de exercícios</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditExercise(int id, ExerciseViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var exercise = await _context.Exercises
                    .Include(e => e.ExerciseMuscleGroups)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (exercise == null)
                {
                    return NotFound();
                }

                exercise.Name = model.Name;
                exercise.Description = model.Description;

                // Remove associações antigas
                _context.ExerciseMuscleGroups.RemoveRange(exercise.ExerciseMuscleGroups);

                // Adiciona novas associações
                if (model.MuscleGroupIds != null && model.MuscleGroupIds.Any())
                {
                    foreach (var muscleGroupId in model.MuscleGroupIds)
                    {
                        var isPrimary = model.PrimaryMuscleGroupIds?.Contains(muscleGroupId) ?? false;
                        
                        var exerciseMuscleGroup = new ExerciseMuscleGroup
                        {
                            ExerciseId = exercise.Id,
                            MuscleGroupId = muscleGroupId,
                            IsPrimary = isPrimary
                        };

                        _context.ExerciseMuscleGroups.Add(exerciseMuscleGroup);
                    }
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Exercises));
            }

            ViewBag.MuscleGroups = await _context.MuscleGroups.OrderBy(mg => mg.Name).ToListAsync();
            return View(model);
        }

        /// <summary>
        /// Exclui um exercício
        /// </summary>
        /// <param name="id">ID do exercício</param>
        /// <returns>Redirecionamento para lista de exercícios</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteExercise(int id)
        {
            var exercise = await _context.Exercises
                .Include(e => e.ExerciseMuscleGroups)
                .Include(e => e.LoggedExercises)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (exercise == null)
            {
                return NotFound();
            }

            // Verifica se há registros de treino associados
            if (exercise.LoggedExercises.Any())
            {
                TempData["ErrorMessage"] = $"Não é possível excluir o exercício '{exercise.Name}' pois ele possui {exercise.LoggedExercises.Count} registro(s) de treino associado(s).";
                return RedirectToAction(nameof(Exercises));
            }

            // Remove associações com grupos musculares
            _context.ExerciseMuscleGroups.RemoveRange(exercise.ExerciseMuscleGroups);
            
            // Remove associações com planos de treino
            var planExercises = await _context.WorkoutPlans
                .SelectMany(wp => wp.PlanExercises)
                .Where(pe => pe.ExerciseId == id)
                .ToListAsync();
            
            if (planExercises.Any())
            {
                _context.Set<WorkoutPlanExercise>().RemoveRange(planExercises);
            }

            // Remove o exercício
            _context.Exercises.Remove(exercise);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Exercício '{exercise.Name}' excluído com sucesso!";
            return RedirectToAction(nameof(Exercises));
        }

        #endregion

        #region Alpha User Registration

        /// <summary>
        /// Registra um novo usuário Alpha mediante autenticação do administrador.
        /// </summary>
        /// <param name="adminUser">Nome de usuário do administrador.</param>
        /// <param name="adminPwd">Senha do administrador.</param>
        /// <param name="newUserName">Nome de usuário do novo usuário.</param>
        /// <param name="newUserMail">Email do novo usuário.</param>
        /// <param name="newUserPwd">Senha do novo usuário.</param>
        /// <returns>
        /// Confirmação com nome, email e senha do usuário criado em caso de sucesso,
        /// ou mensagem de erro caso as credenciais do administrador sejam inválidas.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RegisterAlphaUser(string adminUser, string adminPwd, string newUserName, string newUserMail, string newUserPwd)
        {
            var admin = await _userManager.FindByNameAsync(adminUser)
                ?? (adminUser.Contains('@') ? await _userManager.FindByEmailAsync(adminUser) : null);

            if (admin == null || !await _userManager.IsInRoleAsync(admin, "Admin"))
            {
                return Unauthorized("Credenciais de administrador inválidas.");
            }

            var checkPassword = await _signInManager.CheckPasswordSignInAsync(admin, adminPwd, false);
            if (!checkPassword.Succeeded)
            {
                return Unauthorized("Credenciais de administrador inválidas.");
            }

            var newUser = new User { UserName = newUserName, Email = newUserMail };
            var result = await _userManager.CreateAsync(newUser, newUserPwd);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            return Ok(new { username = newUserName, email = newUserMail, password = newUserPwd });
        }

        #endregion
    }
}
