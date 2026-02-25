/**
 * Gerencia o tema de interface (claro/escuro/sistema) via cookie.
 * O atributo `data-theme` é definido pelo servidor no elemento `<html>`;
 * este script gerencia trocas em tempo real sem recarregar a página.
 */
(function () {
    'use strict';

    /** @type {string} Nome do cookie de tema. */
    const COOKIE_NAME = 'tupiJua_theme';

    /** @type {string[]} Temas válidos. */
    const VALID_THEMES = ['light', 'dark', 'system'];

    /** @type {Record<string, {icon: string, label: string}>} Metadados por tema. */
    const THEME_META = {
        light:  { icon: 'fa-sun',                label: 'Tema claro'   },
        dark:   { icon: 'fa-moon',               label: 'Tema escuro'  },
        system: { icon: 'fa-circle-half-stroke',  label: 'Tema do sistema' }
    };

    /**
     * Grava um cookie com validade de 1 ano.
     * @param {string} name
     * @param {string} value
     */
    function setCookie(name, value) {
        const expires = new Date();
        expires.setFullYear(expires.getFullYear() + 1);
        const secureFlag = (window.location && window.location.protocol === 'https:') ? '; Secure' : '';
        document.cookie = `${name}=${encodeURIComponent(value)}; expires=${expires.toUTCString()}; path=/; SameSite=Lax${secureFlag}`;
    }

    /**
     * Aplica o tema ao elemento `<html>` e persiste no cookie.
     * @param {string} theme - "light", "dark" ou "system".
     */
    function applyTheme(theme) {
        if (!VALID_THEMES.includes(theme)) theme = 'system';
        document.documentElement.setAttribute('data-theme', theme);
        setCookie(COOKIE_NAME, theme);
        _updateUI(theme);
    }

    /**
     * Retorna o tema atual a partir do atributo do elemento `<html>`.
     * @returns {string}
     */
    function getCurrentTheme() {
        return document.documentElement.getAttribute('data-theme') || 'system';
    }

    /**
     * Alterna o tema na ordem: system → light → dark → system.
     * @returns {string} O novo tema aplicado.
     */
    function cycleTheme() {
        const order = ['system', 'light', 'dark'];
        const current = getCurrentTheme();
        const next = order[(order.indexOf(current) + 1) % order.length];
        applyTheme(next);
        return next;
    }

    /**
     * Atualiza o ícone e o aria-label do botão de alternância.
     * @param {string} theme
     */
    function _updateUI(theme) {
        const btn  = document.getElementById('themeToggleBtn');
        const icon = document.getElementById('themeIcon');
        if (!btn || !icon) return;

        const meta = THEME_META[theme] ?? THEME_META.system;
        icon.className = `fas ${meta.icon}`;
        btn.setAttribute('aria-label', meta.label);
        btn.setAttribute('title', meta.label);
    }

    // Expõe a API globalmente
    window.ThemeManager = { applyTheme, getCurrentTheme, cycleTheme };

    // Inicializa o ícone após o DOM estar pronto
    document.addEventListener('DOMContentLoaded', () => _updateUI(getCurrentTheme()));
})();
