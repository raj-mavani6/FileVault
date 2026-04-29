// FileVault - Scroll Animations & Dashboard Counters
document.addEventListener('DOMContentLoaded', () => {
    // Intersection Observer for scroll animations
    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                setTimeout(() => entry.target.classList.add('visible'), index * 80);
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1, rootMargin: '0px 0px -50px 0px' });

    document.querySelectorAll('.fv-animate-on-scroll').forEach(el => observer.observe(el));

    // Counter animation for stat cards
    document.querySelectorAll('[data-count]').forEach(el => {
        const target = parseInt(el.dataset.count);
        if (isNaN(target) || target === 0) return;
        let current = 0;
        const increment = Math.max(1, Math.ceil(target / 40));
        const timer = setInterval(() => {
            current += increment;
            if (current >= target) { current = target; clearInterval(timer); }
            el.textContent = current.toLocaleString();
        }, 30);
    });
});
