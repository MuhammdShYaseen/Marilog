let state = null;

export function attach() {
    if (state) return;

    const getRoot = () => document.querySelector('.wa-viewer.mud-dialog');
    const getInput = () => getRoot()?.querySelector('input[type="file"]');
    const hasFiles = e => Array.from(e.dataTransfer?.types ?? []).includes('Files');

    const push = files => {
        const input = getInput();
        if (!input || !files || files.length === 0) return;
        const dt = new DataTransfer();
        Array.from(files).forEach(f => dt.items.add(f));
        input.files = dt.files;
        input.dispatchEvent(new Event('change', { bubbles: true }));
    };

    const onDragOver = e => {
        if (!hasFiles(e) || e.target === getInput()) return;
        e.preventDefault();
        e.dataTransfer.dropEffect = 'copy';
    };

    const onDrop = e => {
        if (!hasFiles(e) || e.target === getInput()) return; // المربع الحالي بيشتغل لحاله
        e.preventDefault();
        push(e.dataTransfer.files);
    };

    const onPaste = e => {
        const files = e.clipboardData?.files;
        if (!files || files.length === 0) return; // لصق النص عادي
        e.preventDefault();
        push(files);
    };

    document.addEventListener('dragover', onDragOver);
    document.addEventListener('drop', onDrop);
    document.addEventListener('paste', onPaste);
    state = { onDragOver, onDrop, onPaste };
}

export function detach() {
    if (!state) return;
    document.removeEventListener('dragover', state.onDragOver);
    document.removeEventListener('drop', state.onDrop);
    document.removeEventListener('paste', state.onPaste);
    state = null;
}