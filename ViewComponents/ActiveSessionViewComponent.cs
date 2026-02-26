using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TupiJua.Helpers;
using TupiJua.Models;

namespace TupiJua.ViewComponents
{
    /// <summary>
    /// ViewComponent que exibe a sessão de treino ativa (não concluída) mais recente do usuário no dia atual.
    /// </summary>
    public class ActiveSessionViewComponent : ViewComponent
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public ActiveSessionViewComponent(UserManager<User> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        /// <summary>
        /// Busca a sessão ativa mais recente do dia para o usuário logado.
        /// </summary>
        /// <returns>View com a sessão ativa ou null</returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Content(string.Empty);
            }

            var userId = _userManager.GetUserId(HttpContext.User);
            if (userId == null)
            {
                return Content(string.Empty);
            }

            var today = DateTimeHelper.Today;
            var tomorrow = today.AddDays(1);

            // Busca a sessão não concluída mais recente do dia atual
            var activeSession = await _context.WorkoutSessions
                .Where(ws => ws.UserId == userId 
                    && ws.Date >= today 
                    && ws.Date < tomorrow 
                    && !ws.IsCompleted)
                .Include(ws => ws.WorkoutPlan)
                .Include(ws => ws.LoggedExercises)
                .OrderByDescending(ws => ws.Date)
                .FirstOrDefaultAsync();

            if (activeSession == null)
            {
                return Content(string.Empty);
            }

            // Não exibir o card se já estamos visualizando esta sessão
            var routeData = ViewContext.RouteData;
            var controller = routeData.Values["controller"]?.ToString();
            var action = routeData.Values["action"]?.ToString();
            
            if (controller?.Equals("Training", StringComparison.OrdinalIgnoreCase) == true && 
                action?.Equals("ViewWorkout", StringComparison.OrdinalIgnoreCase) == true)
            {
                // Tentar obter sessionId dos RouteValues ou Query String
                var sessionIdValue = routeData.Values["sessionId"] ?? 
                                    HttpContext.Request.Query["sessionId"].FirstOrDefault();
                
                if (sessionIdValue != null && int.TryParse(sessionIdValue.ToString(), out var currentSessionId))
                {
                    if (currentSessionId == activeSession.Id)
                    {
                        return Content(string.Empty);
                    }
                }
            }

            return View(activeSession);
        }
    }
}
