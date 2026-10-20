// fleetMap.js — fleet overview map for the Operations dashboard.
const CDN = 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4';
const instances = new WeakMap();
const creating = new WeakMap();
let leafletLoading = null;
let apiBase = '';
// Base maps per theme brightness — both from Esri, no API key needed
const TILES = {
    light: {
        base: 'ocean-base',
        reference: 'ocean-reference',
        maxZoom: 13,
        attribution: 'Tiles © Esri, GEBCO, NOAA, Garmin, HERE'
    },
    dark: {
        base: 'dark-gray-base',
        reference: 'dark-gray-reference',
        maxZoom: 16,
        attribution: 'Tiles © Esri'
    }
};

function ensureLeaflet() {
    if (window.L) return Promise.resolve();
    if (leafletLoading) return leafletLoading;

    leafletLoading = new Promise((resolve, reject) => {
        if (!document.querySelector(`link[href="${CDN}/leaflet.min.css"]`)) {
            const css = document.createElement('link');
            css.rel = 'stylesheet';
            css.href = `${CDN}/leaflet.min.css`;
            document.head.appendChild(css);
        }
        const script = document.createElement('script');
        script.src = `${CDN}/leaflet.min.js`;
        script.onload = () => resolve();
        script.onerror = () => { leafletLoading = null; reject(new Error('Map library could not be loaded.')); };
        document.head.appendChild(script);
    });
    return leafletLoading;
}

// Vessel name labels follow the app theme through MudBlazor's CSS variables
function ensureLabelStyle() {
    if (document.getElementById('fleet-map-style')) return;
    const style = document.createElement('style');
    style.id = 'fleet-map-style';
    style.textContent =
        '.leaflet-tooltip.fleet-label{background:var(--mud-palette-surface);color:var(--mud-palette-text-primary);' +
        'border:1px solid var(--mud-palette-lines-default);border-radius:2px;box-shadow:none;' +
        'font-size:11px;font-weight:600;padding:1px 6px;white-space:nowrap}' +
        '.leaflet-tooltip.fleet-label::before{display:none}';
    document.head.appendChild(style);
}

function readPalette() {
    const css = getComputedStyle(document.documentElement);
    return {
        accent: css.getPropertyValue('--mud-palette-primary').trim() || '#00796b',
        surface: css.getPropertyValue('--mud-palette-surface').trim() || '#ffffff',
        ink: css.getPropertyValue('--mud-palette-text-primary').trim() || '#1f2937',
        warning: css.getPropertyValue('--mud-palette-warning').trim() || '#f59e0b'
    };
}

// Works with any theme: a dark surface colour means a dark theme.
// The browser converts any colour format (#hex, rgba, named) to "rgb(r, g, b)" for us.
function isDark(surfaceColor) {
    const probe = document.createElement('div');
    probe.style.display = 'none';
    probe.style.color = surfaceColor;
    document.body.appendChild(probe);
    const rgb = (getComputedStyle(probe).color.match(/\d+(\.\d+)?/g) ?? ['255', '255', '255']).map(Number);
    probe.remove();

    const [r, g, b] = rgb;
    return (0.2126 * r + 0.7152 * g + 0.0722 * b) / 255 < 0.5;
}

function applyTiles(instance) {
    for (const layer of instance.tileLayers) instance.map.removeLayer(layer);

    const set = instance.dark ? TILES.dark : TILES.light;
    const esri = key => `${apiBase}/api/map-tiles/${key}/{z}/{y}/{x}`;

    instance.tileLayers = [
        L.tileLayer(esri(set.base), { maxZoom: set.maxZoom, noWrap: true, attribution: set.attribution }),
        L.tileLayer(esri(set.reference), { maxZoom: set.maxZoom, noWrap: true })
    ];
    for (const layer of [...instance.tileLayers].reverse()) layer.addTo(instance.map).bringToBack();

    // Colour shown behind tiles while they load
    instance.element.style.background = instance.palette.surface;
}

const anchorSvg = size =>
    `<svg width="${size}" height="${size}" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="5" r="2.5"/><path d="M12 7.5V21M7 11h10M4 14a8 8 0 0 0 16 0"/></svg>`;
const headingSvg = size =>
    `<svg width="${size}" height="${size}" viewBox="0 0 24 24" fill="currentColor"><path d="M12 2l6 18-6-4-6 4z"/></svg>`;

// Stale positions (older than 24 h) are drawn in the warning colour; the selected vessel gets a ring
function markerHtml(v, palette, selected) {
    const color = v.stale ? palette.warning : palette.accent;
    const ring = selected ? `outline:2px solid ${palette.ink};outline-offset:2px;` : '';

    if (v.stationary) {
        return `<div style="color:${color};width:28px;height:28px;background:${palette.surface};border:2px solid ${color};` +
            `border-radius:50%;display:grid;place-items:center;box-sizing:border-box;${ring}">${anchorSvg(16)}</div>`;
    }
    return `<div style="width:28px;height:28px;border-radius:50%;display:grid;place-items:center;${ring}">` +
        `<div style="color:${color};transform:rotate(${v.course}deg);line-height:0;` +
        `filter:drop-shadow(0 0 1.5px ${palette.surface}) drop-shadow(0 0 1.5px ${palette.surface})">${headingSvg(26)}</div></div>`;
}

function iconFor(v, palette, selected) {
    return L.divIcon({ className: '', html: markerHtml(v, palette, selected), iconSize: [28, 28], iconAnchor: [14, 14] });
}

function fitAll(instance) {
    const points = instance.vessels.map(v => [v.latitude, v.longitude]);
    if (points.length === 0) instance.map.setView([20, 55], 3);
    else if (points.length === 1) instance.map.setView(points[0], 5);
    else instance.map.fitBounds(points, { padding: [50, 50], maxZoom: 6 });
}

function drawVessels(instance, vessels) {
    instance.vessels = vessels ?? [];
    instance.layer.clearLayers();
    instance.markers.clear();

    for (const v of instance.vessels) {
        const marker = L.marker([v.latitude, v.longitude], {
            icon: iconFor(v, instance.palette, v.vesselId === instance.selectedId),
            title: v.name ?? '',
            keyboard: false,
            riseOnHover: true
        });
        marker.bindTooltip(v.name ?? '', { permanent: true, direction: 'right', offset: [14, 0], className: 'fleet-label' });
        marker.on('click', () => instance.dotNetRef?.invokeMethodAsync('OnMarkerClick', v.vesselId));
        marker.addTo(instance.layer);
        instance.markers.set(v.vesselId, { marker, vessel: v });
    }
}

// MudBlazor rewrites a <style> element when the user switches theme — react to that
function watchTheme(instance) {
    let scheduled = false;

    const check = () => {
        scheduled = false;
        const palette = readPalette();
        if (JSON.stringify(palette) === JSON.stringify(instance.palette)) return;

        instance.palette = palette;
        const dark = isDark(palette.surface);
        if (dark !== instance.dark) {
            instance.dark = dark;
            applyTiles(instance);
        } else {
            instance.element.style.background = palette.surface;
        }
        drawVessels(instance, instance.vessels); // recolour markers, selection is kept
    };

    const isStyleNode = n => n?.nodeName === 'STYLE' || n?.parentNode?.nodeName === 'STYLE';
    const isRoot = n => n === document.documentElement || n === document.body;

    instance.themeObserver = new MutationObserver(mutations => {
        if (scheduled) return;
        const relevant = mutations.some(m =>
            m.type === 'attributes' ? isRoot(m.target)
                : isStyleNode(m.target) || [...m.addedNodes].some(isStyleNode));
        if (!relevant) return;
        scheduled = true;
        requestAnimationFrame(check);
    });

    instance.themeObserver.observe(document.documentElement, {
        childList: true,
        subtree: true,
        characterData: true,
        attributes: true,
        attributeFilter: ['style', 'class']
    });
}

async function create(element, dotNetRef) {
    await ensureLeaflet();
    ensureLabelStyle();

    const palette = readPalette();

    const map = L.map(element, {
        zoomControl: false,
        scrollWheelZoom: false,
        minZoom: 2,
        maxBounds: [[-85, -180], [85, 180]],
        maxBoundsViscosity: 1.0,
        worldCopyJump: false
    });
    // Bottom-left, because the vessel card sits on the right side of the map
    L.control.zoom({ position: 'bottomleft' }).addTo(map);
    map.attributionControl.setPrefix(false);

    const layer = L.layerGroup().addTo(map);
    const instance = {
        element, map, layer, palette, dotNetRef,
        dark: isDark(palette.surface),
        tileLayers: [],
        markers: new Map(), vessels: [], selectedId: null,
        observer: null, themeObserver: null, fitted: false
    };
    instances.set(element, instance);

    applyTiles(instance);
    watchTheme(instance);

    map.on('click', () => map.scrollWheelZoom.enable());
    map.on('mouseout', () => map.scrollWheelZoom.disable());

    // Same rule as the vessel map: fit only once the container has a real size
    const hasSize = () => element.clientWidth > 0 && element.clientHeight > 0;
    const fit = () => {
        map.invalidateSize();
        fitAll(instance);
        instance.fitted = true;
    };

    if (hasSize()) fit();

    instance.observer = new ResizeObserver(() => {
        if (!hasSize()) return;
        if (instance.fitted) map.invalidateSize();
        else fit();
    });
    instance.observer.observe(element);
}

export async function render(element, vessels, dotNetRef, apiBaseUrl) {
    apiBase = (apiBaseUrl ?? '').replace(/\/+$/, '');
    // Blazor can call render again while the map library is still loading — create the map only once
    if (!instances.has(element)) {
        if (!creating.has(element)) creating.set(element, create(element, dotNetRef));
        try { await creating.get(element); }
        finally { creating.delete(element); }
    }

    const instance = instances.get(element);
    if (!instance) return;

    instance.dotNetRef = dotNetRef;
    drawVessels(instance, vessels);
    if (instance.fitted) fitAll(instance);
}

export function select(element, vesselId) {
    const instance = instances.get(element);
    if (!instance) return;

    const previous = instance.selectedId;
    instance.selectedId = vesselId ?? null;

    for (const id of [previous, instance.selectedId]) {
        const entry = id == null ? null : instance.markers.get(id);
        if (entry) entry.marker.setIcon(iconFor(entry.vessel, instance.palette, id === instance.selectedId));
    }
}

export function dispose(element) {
    const instance = instances.get(element);
    if (!instance) return;
    instance.observer?.disconnect();
    instance.themeObserver?.disconnect();
    instance.map.remove();
    instances.delete(element);
}