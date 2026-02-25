using Microsoft.AspNetCore.Mvc;
using TupiJua.Helpers;

namespace TupiJua.Controllers;

/// <summary>
/// Gerencia preferências de interface do usuário.
/// </summary>
[Route("[controller]")]
public class PreferencesController : Controller
{
    /// <summary>
    /// Salva a preferência de tema do usuário em um cookie com validade de 1 ano.
    /// </summary>
    /// <param name="theme">Tema desejado: <c>"light"</c>, <c>"dark"</c> ou <c>"system"</c>.</param>
    /// <param name="returnUrl">URL para redirecionar após salvar.</param>
    /// <returns>Redirecionamento para <paramref name="returnUrl"/> ou para a página inicial.</returns>
    [HttpPost("theme")]
    [ValidateAntiForgeryToken]
    public IActionResult SetTheme([FromForm] string theme, [FromForm] string? returnUrl = null)
    {
        if (!ThemeHelper.ValidThemes.Contains(theme))
            return BadRequest("Tema inválido.");

        Response.Cookies.Append(ThemeHelper.CookieName, theme, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            HttpOnly = false,
            Secure = Request.IsHttps
        });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
}
