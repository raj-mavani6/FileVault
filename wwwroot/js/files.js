// FileVault - File Management Operations

async function createFolder() {
    const name = document.getElementById('newFolderName')?.value?.trim();
    if (!name) { showToast('Please enter a folder name', 'warning'); return; }

    try {
        await apiCall('/api/folders', 'POST', {
            name,
            parentFolderId: (typeof currentFolderId !== 'undefined' && currentFolderId) ? currentFolderId : null
        });
        showToast('Folder created!', 'success');
        setTimeout(() => location.reload(), 500);
    } catch (err) {
        showToast(err.message, 'danger');
    }
}

async function renameFolder(id, currentName) {
    const newName = prompt('Rename folder:', currentName);
    if (!newName || newName === currentName) return;

    try {
        await apiCall(`/api/folders/${id}`, 'PUT', { name: newName });
        showToast('Folder renamed!', 'success');
        setTimeout(() => location.reload(), 500);
    } catch (err) {
        showToast(err.message, 'danger');
    }
}

async function deleteFolder(id) {
    if (!confirm('Move this folder to trash?')) return;
    try {
        await apiCall(`/api/folders/${id}`, 'DELETE');
        showToast('Folder moved to trash', 'success');
        setTimeout(() => location.reload(), 500);
    } catch (err) {
        showToast(err.message, 'danger');
    }
}

async function deleteFile(id) {
    if (!confirm('Move this file to trash?')) return;
    try {
        await apiCall(`/api/files/${id}`, 'DELETE');
        showToast('File moved to trash', 'success');
        setTimeout(() => location.reload(), 500);
    } catch (err) {
        showToast(err.message, 'danger');
    }
}

async function restoreFile(id) {
    try {
        await apiCall(`/api/files/${id}/restore`, 'POST');
        showToast('File restored!', 'success');
        setTimeout(() => location.reload(), 500);
    } catch (err) {
        showToast(err.message, 'danger');
    }
}

async function restoreFolder(id) {
    try {
        await apiCall(`/api/folders/${id}/restore`, 'POST');
        showToast('Folder restored!', 'success');
        setTimeout(() => location.reload(), 500);
    } catch (err) {
        showToast(err.message, 'danger');
    }
}

async function permanentDeleteFolder(id) {
    if (!confirm('Permanently delete this folder and ALL files inside? This cannot be undone.')) return;
    try {
        await apiCall(`/api/folders/${id}?permanent=true`, 'DELETE');
        showToast('Folder and all files permanently deleted', 'success');
        setTimeout(() => location.reload(), 500);
    } catch (err) {
        showToast(err.message, 'danger');
    }
}

async function permanentDelete(id) {
    if (!confirm('Permanently delete this file? This cannot be undone.')) return;
    try {
        await apiCall(`/api/files/${id}?permanent=true`, 'DELETE');
        showToast('File permanently deleted', 'success');
        setTimeout(() => location.reload(), 500);
    } catch (err) {
        showToast(err.message, 'danger');
    }
}

async function emptyTrash() {
    if (!confirm('Empty all items from Trash? This cannot be undone.')) return;
    try {
        await apiCall('/api/files/empty-trash', 'POST');
        showToast('Trash emptied', 'success');
        setTimeout(() => location.reload(), 500);
    } catch (err) {
        showToast(err.message, 'danger');
    }
}

// Share
function shareFile(fileId) {
    document.getElementById('shareFileId').value = fileId;
    document.getElementById('shareResult')?.classList.add('d-none');
    document.getElementById('shareCreateBtn')?.classList.remove('d-none');
    new bootstrap.Modal(document.getElementById('shareModal')).show();
}

async function createShareLink() {
    const fileId = document.getElementById('shareFileId').value;
    const expiryDays = parseInt(document.getElementById('shareExpiry').value) || 7;
    const password = document.getElementById('sharePassword').value || null;
    const allowDownload = document.getElementById('shareAllowDownload').checked;

    try {
        const res = await apiCall('/api/shares', 'POST', { fileId, expiryDays, password, allowDownload });
        document.getElementById('shareUrl').value = res.url;
        document.getElementById('shareResult').classList.remove('d-none');
        document.getElementById('shareCreateBtn').classList.add('d-none');
        showToast('Share link created!', 'success');
    } catch (err) {
        showToast(err.message, 'danger');
    }
}

function copyShareUrl() {
    const url = document.getElementById('shareUrl').value;
    navigator.clipboard.writeText(url).then(() => showToast('Link copied!', 'success'));
}
