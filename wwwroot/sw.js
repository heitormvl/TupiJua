/** @type {string} Versão do cache – incrementar ao alterar arquivos cacheados */
const CACHE_VERSION = "v3";
const STATIC_CACHE = `tupijua-static-${CACHE_VERSION}`;
const DYNAMIC_CACHE = `tupijua-dynamic-${CACHE_VERSION}`;

/** Ativos estáticos pré-cacheados na instalação */
const STATIC_ASSETS = [
  "/offline.html",
  "/css/site.css",
  "/js/site.js",
  "/js/rest-timer.js",
  "/js/rest-timer-ui.js",
  "/js/theme.js",
  "/js/training.js",
  "/js/exercise-helpers.js",
  "/lib/bootstrap/dist/css/bootstrap.min.css",
  "/lib/bootstrap/dist/js/bootstrap.bundle.min.js",
  "/lib/jquery/dist/jquery.min.js",
  "/images/android-chrome-192x192.png",
  "/images/android-chrome-512x512.png",
  "/manifest.json"
];

/** Rotas HTML que devem ser cacheadas dinamicamente (Network-first) */
const HTML_ROUTES = ["/", "/WorkoutPlan", "/Training", "/Home", "/User"];

// ---------------------------------------------------------------------------
// Instalação – pré-cacheia ativos estáticos
// ---------------------------------------------------------------------------
self.addEventListener("install", event => {
  event.waitUntil(
    caches.open(STATIC_CACHE)
      .then(cache => cache.addAll(STATIC_ASSETS))
      .then(() => self.skipWaiting())
  );
});

// ---------------------------------------------------------------------------
// Ativação – remove caches antigos
// ---------------------------------------------------------------------------
self.addEventListener("activate", event => {
  const allowedCaches = [STATIC_CACHE, DYNAMIC_CACHE];
  event.waitUntil(
    caches.keys()
      .then(keys => Promise.all(
        keys
          .filter(key => !allowedCaches.includes(key))
          .map(key => caches.delete(key))
      ))
      .then(() => self.clients.claim())
  );
});

// ---------------------------------------------------------------------------
// Fetch – estratégias diferenciadas por tipo de requisição
// ---------------------------------------------------------------------------
self.addEventListener("fetch", event => {
  const { request } = event;
  const url = new URL(request.url);

  // Ignora requisições de outras origens (CDN externo, analytics etc.)
  if (url.origin !== self.location.origin) return;

  // Ignora requisições não-GET
  if (request.method !== "GET") return;

  const isHtmlRequest = request.headers.get("Accept")?.includes("text/html");

  if (isHtmlRequest) {
    // Estratégia Network-first para páginas HTML
    event.respondWith(networkFirstWithOfflineFallback(request));
  } else {
    // Estratégia Cache-first para ativos estáticos
    event.respondWith(cacheFirstWithNetworkFallback(request));
  }
});

/**
 * Network-first: tenta a rede; em caso de falha usa cache; se não houver,
 * mostra a página offline.
 * @param {Request} request
 * @returns {Promise<Response>}
 */
async function networkFirstWithOfflineFallback(request) {
  try {
    const networkResponse = await fetch(request);
    // Cacheia apenas respostas bem-sucedidas
    if (networkResponse.ok) {
      const cache = await caches.open(DYNAMIC_CACHE);
      cache.put(request, networkResponse.clone());
    }
    return networkResponse;
  } catch {
    const cached = await caches.match(request);
    if (cached) return cached;
    return caches.match("/offline.html");
  }
}

/**
 * Cache-first: serve do cache; se não houver, busca na rede e cacheia.
 * @param {Request} request
 * @returns {Promise<Response>}
 */
async function cacheFirstWithNetworkFallback(request) {
  const cached = await caches.match(request);
  if (cached) return cached;

  try {
    const networkResponse = await fetch(request);
    if (networkResponse.ok) {
      const cache = await caches.open(STATIC_CACHE);
      cache.put(request, networkResponse.clone());
    }
    return networkResponse;
  } catch {
    // Sem cache e sem rede – retorna resposta de erro mínima
    return new Response("", { status: 503, statusText: "Service Unavailable" });
  }
}

// ---------------------------------------------------------------------------
// Push Notifications
// ---------------------------------------------------------------------------

/**
 * Exibe uma notificação recebida via push do servidor.
 * O payload deve ser JSON com { title, body, icon, url }.
 */
self.addEventListener("push", event => {
  let data = { title: "TupiJua", body: "Você tem uma atualização.", icon: "/images/android-chrome-192x192.png" };

  try {
    if (event.data) {
      data = { ...data, ...event.data.json() };
    }
  } catch {
    // payload não é JSON – usa defaults
  }

  event.waitUntil(
    self.registration.showNotification(data.title, {
      body: data.body,
      icon: data.icon || "/images/android-chrome-192x192.png",
      badge: "/images/android-chrome-192x192.png",
      tag: data.tag || "tupijua-push",
      data: { url: data.url || "/" },
      requireInteraction: false
    })
  );
});

/**
 * Ao clicar na notificação, abre (ou foca) o app na URL especificada.
 */
self.addEventListener("notificationclick", event => {
  event.notification.close();

  const targetUrl = event.notification.data?.url || "/";

  event.waitUntil(
    self.clients.matchAll({ type: "window", includeUncontrolled: true })
      .then(clientList => {
        const existing = clientList.find(c => c.url === targetUrl && "focus" in c);
        if (existing) return existing.focus();
        return self.clients.openWindow(targetUrl);
      })
  );
});