using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TupiJua.Models;
using TupiJua.ViewModels;

namespace TupiJua.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public UserController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Página inicial do usuário logado
        /// </summary>
        /// <returns>View com os dados do usuário</returns>
        [HttpGet]
        public IActionResult Index()
        {
            var user = _userManager.GetUserAsync(User).Result;

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new UserViewModel
            {
                Username = user.UserName!,
                Email = user.Email!,
                Weight = user.Weight,
                Height = user.Height,
                BirthDate = user.BirthDate
            };
            return View(model);
        }

        // Desativado no Alpha
        /// <summary>
        /// Página de registro de novo usuário
        /// </summary>
        /// <returns>View para registro de usuário</returns>
        // [HttpGet]
        // [AllowAnonymous]
        // public IActionResult Register() => View();

        /// <summary>
        /// Registra um novo usuário
        /// </summary>
        /// <param name="model">Modelo com os dados do usuário a ser registrado</param>
        /// <returns>Redireciona para a página inicial em caso de sucesso ou retorna a view com erros</returns>
        // [HttpPost]
        // [AllowAnonymous]
        // public async Task<IActionResult> Register(RegisterViewModel model)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         var user = new User { UserName = model.Username, Email = model.Email };
        //         var result = await _userManager.CreateAsync(user, model.Password);

        //         if (result.Succeeded)
        //         {
        //             await _signInManager.SignInAsync(user, isPersistent: false);
        //             return RedirectToAction("Index", "Home");
        //         }

        //         foreach (var error in result.Errors)
        //         {
        //             ModelState.AddModelError(string.Empty, error.Description);
        //         }
        //     }

        //     return View(model);
        // }

        /// <summary>
        /// Página de login do usuário
        /// </summary>
        /// <returns>View para login do usuário</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login() => View();

        /// <summary>
        /// Realiza o login do usuário
        /// </summary>
        /// <param name="model">Modelo com os dados para login</param>
        /// <returns>Redireciona para a página inicial em caso de sucesso ou retorna a view com erros</returns>
        [HttpPost]
        [AllowAnonymous]
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

        /// <summary>
        /// Página de edição dos dados do usuário
        /// </summary>
        /// <returns>View para edição dos dados do usuário</returns>
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new EditUserViewModel
            {
                Weight = user.Weight,
                Height = user.Height,
                BirthDate = user.BirthDate
            };

            return View(model);
        }

        /// <summary>
        /// Edita os dados do usuário
        /// </summary>
        /// <param name="model">Modelo com os dados do usuário a ser editado</param>
        /// <returns>Redireciona para a página de perfil em caso de sucesso ou retorna a view com erros</returns>
        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                user.Weight = model.Weight;
                user.Height = model.Height;
                user.BirthDate = model.BirthDate;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        /// <summary>
        /// Realiza o logout do usuário
        /// </summary>
        /// <returns>Redireciona para a página inicial após logout</returns>
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
