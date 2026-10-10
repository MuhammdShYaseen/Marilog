let state = null;
const INTERNAL = 'application/x-wa-internal';

export function attach(dotnetRef) {
    if (state) return;

    let dragged = null; // { kind: 'file' | 'folder', id: number } while an internal drag is in progress

    const getRoot = () => document.querySelector('.wa-viewer.mud-dialog');
    const getInput = () => getRoot()?.querySelector('input[type="file"]');
    const isEditable = el => !!el?.closest?.('input, textarea, [contenteditable="true"]');
    const dropTargetOf = el => el?.closest?.('[data-wa-drop]') ?? null;
    const hasExternalFiles = e => !dragged && Array.from(e.dataTransfer?.types ?? []).includes('Files');
    const clearTargets = () => document.querySelectorAll('.wa-drop-target')
        .forEach(el => el.classList.remove('wa-drop-target'));

    const push = files => {
        const input = getInput();
        if (!input || !files || files.length === 0) return;
        const dt = new DataTransfer();
        Array.from(files).forEach(f => dt.items.add(f));
        input.files = dt.files;
        input.dispatchEvent(new Event('change', { bubbles: true }));
    };

    const onDragStart = e => {
        const source = e.target.closest?.('[data-wa-drag-file], [data-wa-drag-folder]');
        if (!source) return;

        dragged = source.dataset.waDragFile !== undefined
            ? { kind: 'file', id: Number(source.dataset.waDragFile) }
            : { kind: 'folder', id: Number(source.dataset.waDragFolder) };

        e.dataTransfer.setData(INTERNAL, '1'); // Firefox needs setData to start a drag
        e.dataTransfer.effectAllowed = 'move';
    };

    const onDragOver = e => {
        if (dragged) {
            const target = dropTargetOf(e.target);
            if (!target) {
                clearTargets();
                return;
            }
            e.preventDefault(); // marks this element as a valid drop target
            e.dataTransfer.dropEffect = 'move';
            if (!target.classList.contains('wa-drop-target')) {
                clearTargets();
                target.classList.add('wa-drop-target');
            }
            return;
        }

        if (!hasExternalFiles(e) || e.target === getInput()) return;
        e.preventDefault();
        e.dataTransfer.dropEffect = 'copy';
    };

    const onDrop = e => {
        clearTargets();

        if (dragged) {
            e.preventDefault(); // never let an internal drag land in the file input
            const payload = dragged;
            dragged = null;

            const target = dropTargetOf(e.target);
            if (!target) return;

            const raw = target.dataset.waDrop;
            const targetFolderId = raw === 'root' ? null : Number(raw);
            dotnetRef.invokeMethodAsync('OnInternalDrop', payload.kind, payload.id, targetFolderId);
            return;
        }

        if (!hasExternalFiles(e) || e.target === getInput()) return;
        e.preventDefault();
        push(e.dataTransfer.files);
    };

    const onDragEnd = () => {
        dragged = null;
        clearTargets();
    };

    const onKeyDown = e => {
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'x' && !isEditable(e.target))
            dotnetRef.invokeMethodAsync('OnCutShortcut');
    };

    const onPaste = e => {
        const files = e.clipboardData?.files;
        if (files && files.length > 0) {
            e.preventDefault();
            push(files);
            return;
        }
        if (!isEditable(e.target))
            dotnetRef.invokeMethodAsync('OnPasteShortcut');
    };

    document.addEventListener('dragstart', onDragStart);
    document.addEventListener('dragover', onDragOver);
    document.addEventListener('drop', onDrop);
    document.addEventListener('dragend', onDragEnd);
    document.addEventListener('keydown', onKeyDown);
    document.addEventListener('paste', onPaste);
    state = { onDragStart, onDragOver, onDrop, onDragEnd, onKeyDown, onPaste };
}

export function detach() {
    if (!state) return;
    document.removeEventListener('dragstart', state.onDragStart);
    document.removeEventListener('dragover', state.onDragOver);
    document.removeEventListener('drop', state.onDrop);
    document.removeEventListener('dragend', state.onDragEnd);
    document.removeEventListener('keydown', state.onKeyDown);
    document.removeEventListener('paste', state.onPaste);
    state = null;
}