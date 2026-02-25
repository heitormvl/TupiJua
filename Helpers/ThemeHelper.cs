namespace TupiJua.Helpers;

/// <summary>
/// Fornece utilitários para gerenciamento do tema de interface do usuário.
/// </summary>
public static class ThemeHelper
{
    /// <summary>
    /// Nome do cookie que armazena a preferência de tema.
    /// </summary>
    public const string CookieName = "tupiJua_theme";

    /// <summary>
    /// Temas válidos aceitos pela aplicação.
    /// </summary>
    public static readonly IReadOnlyList<string> ValidThemes = ["light", "dark", "system"];

    /// <summary>
    /// Retorna o tema armazenado no cookie ou <c>"system"</c> como padrão.
    /// </summary>
    /// <param name="request">O <see cref="HttpRequest"/> da requisição atual.</param>
    /// <returns>O tema preferido pelo usuário.</returns>
    public static string GetThemeFromCookie(HttpRequest request)
    {
        var theme = request.Cookies[CookieName];
        return ValidThemes.Contains(theme) ? theme! : "system";
    }
}
