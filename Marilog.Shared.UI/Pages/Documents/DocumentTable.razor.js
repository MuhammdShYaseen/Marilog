export function initColumnResize(containerId, storageKey, defaultWidths, minWidth) {
    const container = document.getElementById(containerId);
    if (!container) {
        return null;
    }

    let widths = loadWidths() ?? defaultWidths.slice();

    function loadWidths() {
        try {
            const raw = localStorage.getItem(storageKey);
            if (!raw) return null;

            const parsed = JSON.parse(raw);
            if (Array.isArray(parsed) && parsed.length === defaultWidths.length) {
                return parsed;
            }
        } catch {
            // corrupted or unavailable localStorage value - ignore and fall back
        }
        return null;
    }

    function saveWidths() {
        try {
            localStorage.setItem(storageKey, JSON.stringify(widths));
        } catch {
            // storage full / disabled (e.g. private browsing) - fail silently
        }
    }

    function applyAll() {
        widths.forEach((w, i) => {
            container.style.setProperty(`--col-${i + 1}-width`, `${w}px`);
        });

        const total = widths.reduce((sum, w) => sum + w, 0);
        container.style.setProperty('--doc-grid-total-width', `${total}px`);
    }

    applyAll();

    function attachHandles() {
        const headerRow = container.querySelector('.wa-doc-grid thead tr');
        if (!headerRow) return;

        Array.from(headerRow.querySelectorAll('th')).forEach((th, colIndex) => {
            // colIndex is 0-based and lines up 1:1 with widths[]/nth-child.
            if (colIndex === 0) return; // hierarchy column stays fixed at 48px
            if (th.querySelector('.wa-col-resize-handle')) return; // already attached

            const handle = document.createElement('div');
            handle.className = 'wa-col-resize-handle';
            th.appendChild(handle);

            let startX = 0;
            let startWidth = 0;

            const onMouseMove = (e) => {
                const delta = e.clientX - startX;
                widths[colIndex] = Math.max(minWidth, startWidth + delta);
                applyAll();
            };

            const onMouseUp = () => {
                document.removeEventListener('mousemove', onMouseMove);
                document.removeEventListener('mouseup', onMouseUp);
                handle.classList.remove('wa-col-resize-active');
                saveWidths();
            };

            handle.addEventListener('mousedown', (e) => {
                e.preventDefault();
                e.stopPropagation(); // don't trigger MudDataGrid sort-on-header-click
                startX = e.clientX;
                startWidth = widths[colIndex];
                handle.classList.add('wa-col-resize-active');
                document.addEventListener('mousemove', onMouseMove);
                document.addEventListener('mouseup', onMouseUp);
            });
        });
    }

    attachHandles();

    // MudBlazor can re-render the header (sorting, data refresh, etc.),
    // which recreates the <th> elements and wipes the handles - reattach
    // whenever that happens.
    const observer = new MutationObserver(() => attachHandles());
    observer.observe(container, { childList: true, subtree: true });

    return {
        dispose: () => observer.disconnect(),
        reset: () => {
            widths = defaultWidths.slice();
            applyAll();
            try {
                localStorage.removeItem(storageKey);
            } catch {
                // ignore
            }
        }
    };
}