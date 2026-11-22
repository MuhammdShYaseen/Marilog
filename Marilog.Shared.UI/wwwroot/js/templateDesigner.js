// Document template designer: move and resize boxes on the page, drop palette items onto it,
// nudge the selected box with the arrow keys, pass the Ctrl shortcuts (duplicate, undo, redo) to .NET,
// and load the template fonts into the browser.
//
// The page is drawn at `scale` CSS pixels per PDF point. Everything sent back to .NET is in points,
// rounded to whole points. While a box is being dragged it moves here in the browser only;
// .NET gets the final position once, when the mouse is released.

const PALETTE_TYPE = 'application/x-marilog-template-key';
const MIN_SIZE = 4;          // points — same as DocumentTemplateRules.MinElementSize
const DRAG_THRESHOLD = 3;    // pixels before a click turns into a drag

let state = null;
const loadedFonts = new Set();

export function attach(page, dotnetRef) {
    detach();

    const s = {
        page,
        dotnetRef,
        scale: 1,
        pageWidth: 0,
        pageHeight: 0,
        drag: null,
        grab: null
    };

    // ── Select, move, resize ─────────────────────────────────────────────

    s.onPointerDown = e => {
        if (e.button !== 0) return;

        const box = e.target.closest('[data-el-index]');
        if (!box || !page.contains(box)) {
            if (e.target === page) dotnetRef.invokeMethodAsync('OnElementSelected', -1);
            page.focus({ preventScroll: true });
            return;
        }

        const index = Number(box.dataset.elIndex);
        dotnetRef.invokeMethodAsync('OnElementSelected', index);

        s.drag = {
            box,
            index,
            mode: e.target.closest('[data-resize]') ? 'resize' : 'move',
            startX: e.clientX,
            startY: e.clientY,
            x: Number(box.dataset.x),
            y: Number(box.dataset.y),
            w: Number(box.dataset.w),
            h: Number(box.dataset.h),
            moved: false,
            result: null
        };

        page.focus({ preventScroll: true });
        e.preventDefault(); // no text selection while dragging
    };

    s.onPointerMove = e => {
        const d = s.drag;
        if (!d) return;

        if (!d.moved
            && Math.abs(e.clientX - d.startX) < DRAG_THRESHOLD
            && Math.abs(e.clientY - d.startY) < DRAG_THRESHOLD)
            return;

        d.moved = true;

        const dx = (e.clientX - d.startX) / s.scale;
        const dy = (e.clientY - d.startY) / s.scale;
        let { x, y, w, h } = d;

        if (d.mode === 'move') {
            x = clamp(Math.round(d.x + dx), 0, s.pageWidth - w);
            y = clamp(Math.round(d.y + dy), 0, s.pageHeight - h);
        } else {
            w = clamp(Math.round(d.w + dx), MIN_SIZE, s.pageWidth - x);
            h = clamp(Math.round(d.h + dy), MIN_SIZE, s.pageHeight - y);
        }

        d.result = { x, y, w, h };
        place(d.box, d.result, s.scale);
    };

    s.onPointerUp = () => {
        const d = s.drag;
        s.drag = null;

        if (d && d.moved && d.result)
            dotnetRef.invokeMethodAsync('OnElementChanged', d.index, d.result.x, d.result.y, d.result.w, d.result.h);
    };

    // ── Keyboard (only while the page itself has focus) ─────────────────
    // A text field keeps its own Ctrl+Z, because then the page doesn't have focus.

    s.onKeyDown = e => {
        if (e.target !== page) return;

        if (e.ctrlKey || e.metaKey) {
            const key = e.key.toLowerCase();
            let method = null;

            if (key === 'z' && !e.shiftKey) method = 'OnUndo';
            else if (key === 'y' || (key === 'z' && e.shiftKey)) method = 'OnRedo';
            else if (key === 'd') method = 'OnDuplicateSelected';   // also stops the browser's "bookmark this page"

            if (method) {
                e.preventDefault();
                dotnetRef.invokeMethodAsync(method);
            }
            return;
        }

        const step = e.shiftKey ? 10 : 1;
        const moves = {
            ArrowLeft: [-step, 0],
            ArrowRight: [step, 0],
            ArrowUp: [0, -step],
            ArrowDown: [0, step]
        };

        if (moves[e.key]) {
            e.preventDefault();
            dotnetRef.invokeMethodAsync('OnNudge', moves[e.key][0], moves[e.key][1]);
        } else if (e.key === 'Delete') {
            e.preventDefault();
            dotnetRef.invokeMethodAsync('OnDeleteSelected');
        }
    };

    // ── Drag from the palette onto the page ─────────────────────────────

    s.onDragStart = e => {
        const item = e.target.closest?.('[data-palette-key]');
        if (!item) return;

        e.dataTransfer.setData(PALETTE_TYPE, item.dataset.paletteKey);
        e.dataTransfer.setData('text/plain', ''); // Firefox starts a drag only with some data
        e.dataTransfer.effectAllowed = 'copy';

        // The drag image follows the cursor at the point where the item was grabbed,
        // so the new box must land at that offset too, not with its corner on the cursor.
        const r = item.getBoundingClientRect();
        s.grab = { x: e.clientX - r.left, y: e.clientY - r.top };
    };

    s.onDragOver = e => {
        if (!Array.from(e.dataTransfer?.types ?? []).includes(PALETTE_TYPE)) return;
        e.preventDefault();
        e.dataTransfer.dropEffect = 'copy';
    };

    s.onDrop = e => {
        const key = e.dataTransfer?.getData(PALETTE_TYPE);
        if (!key) return;

        e.preventDefault();
        const rect = page.getBoundingClientRect();
        const grab = s.grab ?? { x: 0, y: 0 };
        s.grab = null;
        const x = Math.round((e.clientX - grab.x - rect.left) / s.scale);
        const y = Math.round((e.clientY - grab.y - rect.top) / s.scale);

        page.focus({ preventScroll: true });
        dotnetRef.invokeMethodAsync('OnPaletteDropped', Number(key), x, y);
    };

    page.addEventListener('pointerdown', s.onPointerDown);
    page.addEventListener('keydown', s.onKeyDown);
    page.addEventListener('dragover', s.onDragOver);
    page.addEventListener('drop', s.onDrop);
    window.addEventListener('pointermove', s.onPointerMove);
    window.addEventListener('pointerup', s.onPointerUp);
    window.addEventListener('pointercancel', s.onPointerUp);
    document.addEventListener('dragstart', s.onDragStart);

    state = s;
}

// Called after every render where the zoom or the page size changed.
export function configure(page, scale, pageWidth, pageHeight) {
    if (!state || state.page !== page) return;
    state.scale = scale;
    state.pageWidth = pageWidth;
    state.pageHeight = pageHeight;
}

export function detach() {
    if (!state) return;

    const s = state;
    state = null;

    s.page.removeEventListener('pointerdown', s.onPointerDown);
    s.page.removeEventListener('keydown', s.onKeyDown);
    s.page.removeEventListener('dragover', s.onDragOver);
    s.page.removeEventListener('drop', s.onDrop);
    window.removeEventListener('pointermove', s.onPointerMove);
    window.removeEventListener('pointerup', s.onPointerUp);
    window.removeEventListener('pointercancel', s.onPointerUp);
    document.removeEventListener('dragstart', s.onDragStart);
}

// Registers the template fonts (the same files the PDF uses) so the page shows the text exactly as it will print.
export async function loadFonts(fonts) {
    for (const font of fonts ?? []) {
        if (loadedFonts.has(font.url)) continue;
        loadedFonts.add(font.url);

        try {
            const face = new FontFace(font.family, `url("${font.url}")`, {
                weight: String(font.weight),
                style: font.italic ? 'italic' : 'normal'
            });
            document.fonts.add(await face.load());
        } catch {
            loadedFonts.delete(font.url); // try again next time
        }
    }
}

function place(box, r, scale) {
    box.style.left = `${r.x * scale}px`;
    box.style.top = `${r.y * scale}px`;
    box.style.width = `${r.w * scale}px`;
    box.style.height = `${r.h * scale}px`;
}

function clamp(value, min, max) {
    return Math.min(Math.max(value, min), Math.max(min, max));
}