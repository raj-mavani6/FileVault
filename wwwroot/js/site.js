// FileVault - Global Scripts

function showToast(message, type = 'info') {
    const container = document.getElementById('toastContainer');
    if (!container) return;

    const icons = { 
        success: 'bi-check-circle-fill', 
        danger: 'bi-exclamation-triangle-fill', 
        info: 'bi-info-circle-fill', 
        warning: 'bi-exclamation-circle-fill' 
    };
    
    // Vibrant colors that pop in dark mode
    const colors = { 
        success: '#2ecc71', 
        danger: '#ff4757', 
        info: '#3742fa', 
        warning: '#ffa502' 
    };

    const toast = document.createElement('div');
    toast.className = 'toast fv-toast show';
    toast.setAttribute('role', 'alert');
    toast.innerHTML = `
        <div class="toast-body d-flex align-items-center gap-3">
            <div class="fv-toast-icon-wrapper" style="color:${colors[type] || colors.info};">
                <i class="bi ${icons[type] || icons.info}" style="font-size:1.4rem;"></i>
            </div>
            <div class="flex-grow-1">
                <span class="fw-600" style="color: var(--text-primary); font-size: 0.9rem;">${message}</span>
            </div>
            <button type="button" class="btn-close" onclick="this.closest('.toast').remove()"></button>
        </div>
    `;
    container.appendChild(toast);
    setTimeout(() => {
        toast.classList.add('hide');
        setTimeout(() => toast.remove(), 400);
    }, 5000);
}

async function apiCall(url, method = 'GET', body = null) {
    const opts = {
        method,
        headers: { 'Content-Type': 'application/json' },
        credentials: 'same-origin'
    };
    if (body) opts.body = JSON.stringify(body);
    const res = await fetch(url, opts);
    if (!res.ok) {
        const err = await res.json().catch(() => ({ error: 'Request failed' }));
        throw new Error(err.error || 'Request failed');
    }
    return res.json();
}
