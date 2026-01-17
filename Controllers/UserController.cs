using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TupiJua.Models;
using TupiJua.ViewModels;

namespace TupiJua.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string userNameToLogin = model.Username;
                
                // Se o usuário inseriu um email, buscar o nome de usuário correspondente
                if (model.Username.Contains("@"))
                {
                    var userByEmail = await _userManager.FindByEmailAsync(model.Username);
                    if (userByEmail != null && !string.IsNullOrEmpty(userByEmail.UserName))
                    {
                        userNameToLogin = userByEmail.UserName;
                    }
                }

                var result = await _signInManager.PasswordSignInAsync(userNameToLogin, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Login inválido.");
            }
            return View(model);
        }
    }
}
