const CDN = 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4';
const instances = new WeakMap();
const pendingWind = new WeakMap();
let leafletLoading = null;

// Base maps per theme brightness — both from Esri, no API key needed
const TILES = {
    light: {
        base: 'Ocean/World_Ocean_Base',
        reference: 'Ocean/World_Ocean_Reference',
        maxZoom: 13,
        attribution: 'Tiles © Esri, GEBCO, NOAA, Garmin, HERE'
    },
    dark: {
        base: 'Canvas/World_Dark_Gray_Base',
        reference: 'Canvas/World_Dark_Gray_Reference',
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

function readPalette() {
    const css = getComputedStyle(document.documentElement);
    return {
        accent: css.getPropertyValue('--mud-palette-primary').trim() || '#00796b',
        surface: css.getPropertyValue('--mud-palette-surface').trim() || '#ffffff',
        ink: css.getPropertyValue('--mud-palette-text-primary').trim() || '#1f2937',
        warning: css.getPropertyValue('--mud-palette-warning').trim() || '#f59e0b',
        error: css.getPropertyValue('--mud-palette-error').trim() || '#dc2626'
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
    const esri = path => `https://server.arcgisonline.com/ArcGIS/rest/services/${path}/MapServer/tile/{z}/{y}/{x}`;

    const base = L.tileLayer(esri(set.base), { maxZoom: set.maxZoom, noWrap: true, attribution: set.attribution });
    const reference = L.tileLayer(esri(set.reference), { maxZoom: set.maxZoom, noWrap: true });
    instance.tileLayers = [base, reference];

    // Send the labels back first, then the base behind them — so the order is base, labels, then seamarks on top
    reference.addTo(instance.map).bringToBack();
    base.addTo(instance.map).bringToBack();

    // Colour shown behind tiles while they load
    instance.element.style.background = instance.palette.surface;
}

const anchorSvg = size =>
    `<svg width="${size}" height="${size}" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="5" r="2.5"/><path d="M12 7.5V21M7 11h10M4 14a8 8 0 0 0 16 0"/></svg>`;
const headingSvg = size =>
    `<svg width="${size}" height="${size}" viewBox="0 0 24 24" fill="currentColor"><path d="M12 2l6 18-6-4-6 4z"/></svg>`;

const POINTS = ['N', 'NNE', 'NE', 'ENE', 'E', 'ESE', 'SE', 'SSE', 'S', 'SSW', 'SW', 'WSW', 'W', 'WNW', 'NW', 'NNW'];
const compass = deg => POINTS[Math.round((((deg % 360) + 360) % 360) / 22.5) % 16];

// Arrow points where the wind is blowing TO (same as the weather card), with the speed in knots below.
// Stronger wind draws a thicker arrow; under 2.5 kn shows a small calm circle.
function windArrowSvg(speedKn, directionDeg, color, halo) {
    const size = 44, cx = 22, cy = 18;
    const label =
        `<text x="${cx}" y="${size - 2}" text-anchor="middle" font-size="11" font-weight="600" ` +
        `fill="${color}" stroke="${halo}" stroke-width="3" paint-order="stroke">${Math.round(speedKn)}</text>`;

    if (speedKn < 2.5) {
        return `<svg width="${size}" height="${size}" viewBox="0 0 ${size} ${size}" style="overflow:visible">` +
            `<circle cx="${cx}" cy="${cy}" r="4" fill="none" stroke="${halo}" stroke-width="4"/>` +
            `<circle cx="${cx}" cy="${cy}" r="4" fill="none" stroke="${color}" stroke-width="1.6"/>` +
            `${label}</svg>`;
    }

    const width = 1.8 + Math.min(speedKn / 15, 1.6);
    const arrow = `<path d="M${cx} ${cy + 9} L${cx} ${cy - 9} M${cx - 5} ${cy - 3} L${cx} ${cy - 9} L${cx + 5} ${cy - 3}"/>`;
    const group = (paint, w) =>
        `<g transform="rotate(${directionDeg + 180} ${cx} ${cy})" fill="none" stroke="${paint}" ` +
        `stroke-width="${w}" stroke-linecap="round" stroke-linejoin="round">${arrow}</g>`;

    // A halo in the surface colour keeps the arrow readable over land and sea alike
    return `<svg width="${size}" height="${size}" viewBox="0 0 ${size} ${size}" style="overflow:visible">` +
        `${group(halo, width + 2.5)}${group(color, width)}${label}</svg>`;
}

function vesselIcon(data, palette) {
    const html = data.stationary
        ? `<div style="color:${palette.accent};width:32px;height:32px;background:${palette.surface};border:2px solid ${palette.accent};border-radius:50%;display:grid;place-items:center">${anchorSvg(18)}</div>`
        : `<div style="color:${palette.accent};transform:rotate(${data.course}deg)">${headingSvg(32)}</div>`;
    return L.divIcon({ className: '', html, iconSize: [32, 32], iconAnchor: [16, 16] });
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

        // Recolour everything drawn with the old palette
        instance.area.setStyle({ color: palette.accent, fillColor: palette.accent });
        instance.vesselMarker.setIcon(vesselIcon(instance.data, palette));
        applyWind(instance, instance.windPoints);
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

export async function render(element, data) {
    await ensureLeaflet();
    teardown(element);

    const palette = readPalette();

    const map = L.map(element, {
        zoomControl: false,
        scrollWheelZoom: false,
        minZoom: 2,
        maxBounds: [[-85, -180], [85, 180]],
        maxBoundsViscosity: 1.0,
        worldCopyJump: false
    });
    L.control.zoom({ position: 'bottomright' }).addTo(map);
    map.attributionControl.setPrefix(false);

    const seamarks = L.tileLayer('https://tiles.openseamap.org/seamark/{z}/{x}/{y}.png', {
        maxZoom: 18, noWrap: true, opacity: 0.9, attribution: '© OpenSeaMap'
    }).addTo(map);

    const windLayer = L.layerGroup().addTo(map);
    L.control.layers(null, { 'Wind': windLayer, 'Seamarks': seamarks }, { position: 'topleft', collapsed: false }).addTo(map);

    const lat = data.latitude, lon = data.longitude, half = data.precisionDegrees / 2;
    const bounds = [[lat - half, lon - half], [lat + half, lon + half]];
    const area = L.rectangle(bounds, { color: palette.accent, weight: 2, dashArray: '6 6', fillColor: palette.accent, fillOpacity: 0.08 }).addTo(map);

    const vesselMarker = L.marker([lat, lon], {
        icon: vesselIcon(data, palette),
        title: data.name ?? '',
        keyboard: false,
        zIndexOffset: 1000 // the vessel always sits above the wind arrows
    }).addTo(map);

    const instance = {
        element, map, windLayer, palette, data, area, vesselMarker,
        dark: isDark(palette.surface),
        tileLayers: [],
        windPoints: [],
        observer: null,
        themeObserver: null
    };

    applyTiles(instance);
    watchTheme(instance);

    map.on('click', () => map.scrollWheelZoom.enable());
    map.on('mouseout', () => map.scrollWheelZoom.disable());

    // Leaflet calculates the zoom from the container size. If the container has no size yet
    // (drawer still opening, tab not laid out), fitting now gives zoom 0 — so fit on the first real size.
    let fitted = false;
    const hasSize = () => element.clientWidth > 0 && element.clientHeight > 0;
    const fit = () => {
        map.invalidateSize();
        map.fitBounds(bounds, { padding: [40, 40], maxZoom: 7 }); // zoom 7 keeps the whole wind grid in view
        fitted = true;
    };

    if (hasSize()) fit();

    instance.observer = new ResizeObserver(() => {
        if (!hasSize()) return;
        if (fitted) map.invalidateSize();
        else fit();
    });
    instance.observer.observe(element);

    instances.set(element, instance);

    // Wind data that arrived while the map was still being created
    if (pendingWind.has(element)) {
        applyWind(instance, pendingWind.get(element));
        pendingWind.delete(element);
    }
}

export function setWind(element, points) {
    const instance = instances.get(element);
    if (!instance) { pendingWind.set(element, points); return; }
    applyWind(instance, points);
}

function applyWind(instance, points) {
    instance.windPoints = points ?? [];
    instance.windLayer.clearLayers();

    for (const p of instance.windPoints) {
        if (p.windSpeedKn == null || p.windDirectionDeg == null) continue;

        const bf = p.beaufort ?? 0;
        const color = bf >= 8 ? instance.palette.error : bf >= 6 ? instance.palette.warning : instance.palette.ink;

        L.marker([p.latitude, p.longitude], {
            icon: L.divIcon({
                className: '',
                html: windArrowSvg(p.windSpeedKn, p.windDirectionDeg, color, instance.palette.surface),
                iconSize: [44, 44],
                iconAnchor: [22, 18]
            }),
            title: `${Math.round(p.windSpeedKn)} kn from ${compass(p.windDirectionDeg)} · Force ${bf}`,
            keyboard: false
        }).addTo(instance.windLayer);
    }
}

function teardown(element) {
    const instance = instances.get(element);
    if (!instance) return;
    instance.observer?.disconnect();
    instance.themeObserver?.disconnect();
    instance.map.remove();
    instances.delete(element);
}

export function dispose(element) {
    teardown(element);
    pendingWind.delete(element);
}