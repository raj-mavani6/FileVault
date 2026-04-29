// FileVault - Theme Toggle
(function() {
    const saved = localStorage.getItem('fv-theme') || 'dark';
    document.documentElement.setAttribute('data-theme', saved);

    document.addEventListener('DOMContentLoaded', () => {
        const btns = document.querySelectorAll('#themeToggle, #appThemeToggle');
        btns.forEach(btn => {
            if (!btn) return;
            updateIcon(btn);
            btn.addEventListener('click', () => {
                const current = document.documentElement.getAttribute('data-theme');
                const next = current === 'dark' ? 'light' : 'dark';
                document.documentElement.setAttribute('data-theme', next);
                localStorage.setItem('fv-theme', next);
                btns.forEach(b => updateIcon(b));
            });
        });
    });

    function updateIcon(btn) {
        const theme = document.documentElement.getAttribute('data-theme');
        const icon = btn.querySelector('i');
        if (icon) {
            icon.className = theme === 'dark' ? 'bi bi-sun-fill' : 'bi bi-moon-stars-fill';
        }
    }
})();
