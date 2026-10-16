const CDN = 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4';
const instances = new WeakMap();
let leafletLoading = null;

function ensureLeaflet() {
    if (window.L) return Promise.resolve();
    if (leafletLoading) return leafletLoading;

    leafletLoading = new Promise((resolve, reject) => {
        const css = document.createElement('link');
        css.rel = 'stylesheet';
        css.href = `${CDN}/leaflet.min.css`;
        document.head.appendChild(css);

        const script = document.createElement('script');
        script.src = `${CDN}/leaflet.min.js`;
        script.onload = () => resolve();
        script.onerror = () => { leafletLoading = null; reject(new Error('Map library could not be loaded.')); };
        document.head.appendChild(script);
    });
    return leafletLoading;
}

const anchorSvg = size =>
    `<svg width="${size}" height="${size}" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="5" r="2.5"/><path d="M12 7.5V21M7 11h10M4 14a8 8 0 0 0 16 0"/></svg>`;
const headingSvg = size =>
    `<svg width="${size}" height="${size}" viewBox="0 0 24 24" fill="currentColor"><path d="M12 2l6 18-6-4-6 4z"/></svg>`;

export async function render(element, data) {
    await ensureLeaflet();
    dispose(element);

    const css = getComputedStyle(document.documentElement);
    const accent = css.getPropertyValue('--mud-palette-primary').trim() || '#00796b';
    const surface = css.getPropertyValue('--mud-palette-surface').trim() || '#ffffff';

    const map = L.map(element, { zoomControl: false, scrollWheelZoom: false });
    L.control.zoom({ position: 'bottomright' }).addTo(map);
    map.attributionControl.setPrefix(false);

    const esri = path => `https://server.arcgisonline.com/ArcGIS/rest/services/${path}/MapServer/tile/{z}/{y}/{x}`;
    L.tileLayer(esri('Ocean/World_Ocean_Base'), { maxZoom: 13, attribution: 'Tiles © Esri, GEBCO, NOAA, Garmin, HERE' }).addTo(map);
    L.tileLayer(esri('Ocean/World_Ocean_Reference'), { maxZoom: 13 }).addTo(map);
    L.tileLayer('https://tiles.openseamap.org/seamark/{z}/{x}/{y}.png', { maxZoom: 18, opacity: 0.9, attribution: '© OpenSeaMap' }).addTo(map);

    const lat = data.latitude, lon = data.longitude, half = data.precisionDegrees / 2;
    const bounds = [[lat - half, lon - half], [lat + half, lon + half]];
    L.rectangle(bounds, { color: accent, weight: 2, dashArray: '6 6', fillColor: accent, fillOpacity: 0.08 }).addTo(map);

    const html = data.stationary
        ? `<div style="color:${accent};width:32px;height:32px;background:${surface};border:2px solid ${accent};border-radius:50%;display:grid;place-items:center">${anchorSvg(18)}</div>`
        : `<div style="color:${accent};transform:rotate(${data.course}deg)">${headingSvg(32)}</div>`;

    L.marker([lat, lon], {
        icon: L.divIcon({ className: '', html, iconSize: [32, 32], iconAnchor: [16, 16] }),
        title: data.name ?? '',
        keyboard: false
    }).addTo(map);

    map.fitBounds(bounds, { padding: [40, 40] });
    map.on('click', () => map.scrollWheelZoom.enable());
    map.on('mouseout', () => map.scrollWheelZoom.disable());

    // الـ drawer بيتحرّك والـ tabs بتتبدّل — خلّي الخريطة تعيد حساب حجمها
    const observer = new ResizeObserver(() => map.invalidateSize());
    observer.observe(element);

    instances.set(element, { map, observer });
}

export function dispose(element) {
    const instance = instances.get(element);
    if (!instance) return;
    instance.observer.disconnect();
    instance.map.remove();
    instances.delete(element);
}