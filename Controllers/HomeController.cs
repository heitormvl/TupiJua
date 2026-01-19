using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TupiJua.Models;

namespace TupiJua.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public HomeController(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Exibe a página inicial com planos de treino ativos e últimas sessões.
        /// </summary>
        /// <returns>View com dados dos planos e sessões</returns>
        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);

                var activePlans = await _context.WorkoutPlans
                    .Where(wp => wp.UserId == userId && wp.IsActive)
                    .Include(wp => wp.PlanExercises)
                    .ThenInclude(pe => pe.Exercise)
                    .OrderByDescending(wp => wp.CreatedAt)
                    .ToListAsync();

                var recentSessions = await _context.WorkoutSessions
                    .Where(ws => ws.UserId == userId)
                    .Include(ws => ws.LoggedExercises)
                    .ThenInclude(le => le.Exercise)
                    .Include(ws => ws.WorkoutPlan)
                    .OrderByDescending(ws => ws.Date)
                    .Take(3)
                    .ToListAsync();

                return View((activePlans, recentSessions));
            }

            return View((new List<WorkoutPlan>(), new List<WorkoutSession>()));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
