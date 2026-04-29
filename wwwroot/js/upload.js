// FileVault - Chunked Upload Engine
const CHUNK_SIZE = 5 * 1024 * 1024; // 5MB
const uploads = new Map();
let uploadCounter = 0;

document.addEventListener('DOMContentLoaded', () => {
    const dropZone = document.getElementById('dropZone');
    const fileInput = document.getElementById('fileInput');
    if (!dropZone || !fileInput) return;

    // Drag and drop
    ['dragenter', 'dragover'].forEach(e => dropZone.addEventListener(e, ev => { ev.preventDefault(); dropZone.classList.add('drag-over'); }));
    ['dragleave', 'drop'].forEach(e => dropZone.addEventListener(e, ev => { ev.preventDefault(); dropZone.classList.remove('drag-over'); }));
    dropZone.addEventListener('drop', ev => handleFiles(ev.dataTransfer.files));
    dropZone.addEventListener('click', () => fileInput.click());
    fileInput.addEventListener('change', () => handleFiles(fileInput.files));
});

function handleFiles(files) {
    Array.from(files).forEach(file => startUpload(file));
}

async function startUpload(file) {
    const id = ++uploadCounter;
    const uploadList = document.getElementById('uploadList');
    const uploadItems = document.getElementById('uploadItems');
    uploadList.classList.remove('d-none');

    const item = document.createElement('div');
    item.className = 'fv-upload-item';
    item.id = `upload-${id}`;
    item.innerHTML = `
        <i class="bi bi-file-earmark fs-4 tw-text-accent"></i>
        <div class="fv-upload-info">
            <div class="fv-upload-name">${escapeHtml(file.name)}</div>
            <div class="fv-upload-meta"><span id="upload-status-${id}">Initializing...</span> · ${formatSize(file.size)}</div>
            <div class="fv-upload-progress"><div class="fv-upload-progress-bar" id="upload-bar-${id}" style="width:0%"></div></div>
        </div>
        <div class="d-flex gap-1">
            <button class="btn btn-sm fv-btn-icon" id="upload-pause-${id}" onclick="togglePause(${id})" title="Pause"><i class="bi bi-pause-fill"></i></button>
            <button class="btn btn-sm fv-btn-icon text-danger" onclick="cancelUpload(${id})" title="Cancel"><i class="bi bi-x-lg"></i></button>
        </div>
    `;
    uploadItems.appendChild(item);

    const state = { id, file, sessionId: null, paused: false, cancelled: false, chunksDone: 0, totalChunks: 0 };
    uploads.set(id, state);

    try {
        // Initiate session
        const res = await apiCall('/api/uploads/initiate', 'POST', {
            fileName: file.name,
            contentType: file.type || 'application/octet-stream',
            totalSize: file.size,
            chunkSize: CHUNK_SIZE,
            folderId: (typeof uploadFolderId !== 'undefined' && uploadFolderId) ? uploadFolderId : null
        });

        state.sessionId = res.sessionId;
        state.totalChunks = res.totalChunks;
        updateStatus(id, `Uploading 0/${state.totalChunks} chunks`);

        // Upload chunks
        for (let i = 0; i < state.totalChunks; i++) {
            while (state.paused) await sleep(500);
            if (state.cancelled) return;

            const start = i * CHUNK_SIZE;
            const end = Math.min(start + CHUNK_SIZE, file.size);
            const chunk = file.slice(start, end);

            await fetch(`/api/uploads/${state.sessionId}/chunk/${i}`, {
                method: 'PUT',
                body: chunk,
                credentials: 'same-origin'
            });

            state.chunksDone = i + 1;
            const pct = Math.round((state.chunksDone / state.totalChunks) * 100);
            updateProgress(id, pct);
            updateStatus(id, `Uploading ${state.chunksDone}/${state.totalChunks} chunks (${pct}%)`);
        }

        // Complete
        updateStatus(id, '<span class="text-success fw-bold">✓ Finalizing...</span>');
        const result = await apiCall(`/api/uploads/${state.sessionId}/complete`, 'POST');
        updateStatus(id, `<span class="text-success fw-bold">✓ Upload Complete</span> · ${formatSize(file.size)}`);
        updateProgress(id, 100);
        document.getElementById(`upload-pause-${id}`)?.remove();
        showToast(`${file.name} uploaded successfully!`, 'success');
    } catch (err) {
        if (!state.cancelled) {
            updateStatus(id, `<span class="text-danger fw-bold">✗ Failed:</span> ${err.message}`);
            showToast(`Upload failed: ${err.message}`, 'danger');
        }
    }
}

function togglePause(id) {
    const state = uploads.get(id);
    if (!state) return;
    state.paused = !state.paused;
    const btn = document.getElementById(`upload-pause-${id}`);
    if (btn) btn.innerHTML = state.paused ? '<i class="bi bi-play-fill"></i>' : '<i class="bi bi-pause-fill"></i>';
    updateStatus(id, state.paused ? 'Paused' : `Uploading ${state.chunksDone}/${state.totalChunks} chunks`);
}

async function cancelUpload(id) {
    const state = uploads.get(id);
    if (!state) return;
    state.cancelled = true;
    state.paused = false;
    if (state.sessionId) {
        await fetch(`/api/uploads/${state.sessionId}`, { method: 'DELETE', credentials: 'same-origin' });
    }
    updateStatus(id, 'Cancelled');
    document.getElementById(`upload-${id}`)?.classList.add('opacity-50');
}

function pauseAll() { uploads.forEach((s) => { s.paused = true; }); }
function resumeAll() { uploads.forEach((s) => { s.paused = false; }); }

function updateProgress(id, pct) {
    const bar = document.getElementById(`upload-bar-${id}`);
    if (bar) bar.style.width = pct + '%';
}

function updateStatus(id, html) {
    const el = document.getElementById(`upload-status-${id}`);
    if (el) el.innerHTML = html;
}

function formatSize(bytes) {
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
    let i = 0; let s = bytes;
    while (s >= 1024 && i < sizes.length - 1) { s /= 1024; i++; }
    return s.toFixed(1) + ' ' + sizes[i];
}

function escapeHtml(str) {
    const d = document.createElement('div');
    d.textContent = str;
    return d.innerHTML;
}

function sleep(ms) { return new Promise(r => setTimeout(r, ms)); }
