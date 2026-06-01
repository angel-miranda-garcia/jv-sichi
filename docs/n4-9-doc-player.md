# Sichi Digital Signage — Player TV
## Documentación Node 4.9

> Versión: 1.0 · Estado: COMMITTED · Commit: a558138  
> Stack: HTML + CSS + Vanilla JS (sin framework)  
> Entorno: Chrome en modo kiosko · Android TV Box

---

## 1. Estructura de archivos

```
player/
├── index.html       Shell UI: waiting/error screens + <video>
├── styles.css       Identidad Sichi (#121212 / #E53935 / Inter)
├── indexeddb.js     window.SichiDB — config, playlist, blobs
├── cache.js         Descarga assets + verificación SHA256
├── playlist.js      syncPlaylist() — descarga solo blobs nuevos
├── renderer.js      Carrusel fullscreen loop infinito
├── signalr.js       Hub /signalr/player — playlistChanged / forceRefresh
├── heartbeat.js     POST /api/player/heartbeat cada 30s
└── app.js           Bootstrap completo + pantallas de estado
```

---

## 2. Uso en TV Box

```
http://player.sichi.com/index.html?screen=DEVICE-XXXX
```

Parámetros de URL:

| Parámetro | Requerido | Default | Descripción |
|-----------|-----------|---------|-------------|
| `?screen=` | ✅ Sí | — | ScreenKey del dispositivo. Sin este parámetro → pantalla de error |
| `&api=` | No | `http://api.sichi.com` | URL base de la API. Útil en desarrollo local |

**Ejemplo desarrollo local:**
```
http://localhost:3001/index.html?screen=TV-0001&api=http://api.sichi.com
```

---

## 3. IndexedDB

**Nombre de BD:** `sichi_player` · **Versión:** 1

| Store | KeyPath | Contenido |
|-------|---------|-----------|
| `config` | `key` | `{ key: 'screenKey' \| 'apiUrl' \| 'lastKnownVersion', value }` |
| `playlist` | `id` | `{ id: 1, version, items: [{ mediaItemId, filePath, checksum, duration }] }` |
| `media_blobs` | `mediaItemId` | `{ mediaItemId, blob, checksum }` |

**API expuesta (`window.SichiDB`):**

```js
initDB()
getConfig(key) / setConfig(key, value)
savePlaylist(playlist) / getPlaylist()
saveBlob(mediaItemId, blob, checksum) / getBlob(mediaItemId)
clearBlobs()
```

---

## 4. Flujo de arranque

```
URL ?screen=xxx
      │
      ▼
  initDB()
      │
      ▼
  POST /api/player/register
      │
      ├─ IsApproved = false → pantalla de espera → poll cada 10s
      │
      ├─ IsApproved = true
      │       │
      │       ▼
      │   connectSignalR(screenKey)
      │       │
      │       ▼
      │   GET /api/player/version
      │       │
      │       ├─ remoteVersion > localVersion → syncPlaylist()
      │       │                                      │
      │       └─ igual → ──────────────────────────┐ │
      │                                            ▼ ▼
      │                                       renderer.start()
      │                                            │
      │                                       heartbeat.start()
      │
      └─ Error de red → getPlaylist() desde IndexedDB
              │
              ├─ Existe → renderer.start() (modo offline)
              └─ Vacía  → pantalla de espera
```

---

## 5. Sync de playlist

**`syncPlaylist(screenKey)`** en `playlist.js`:

1. `GET /api/player/playlist?screen={screenKey}`
2. Ordenar ítems por `sortOrder`
3. Por cada ítem:
   - Si blob en IndexedDB y checksum coincide → **skip**
   - Si no → `downloadAndCache(ítem)`
4. Guardar playlist en IndexedDB
5. Actualizar `lastKnownVersion` en config

> **Regla de oro:** `renderer.start()` solo se llama cuando **todos** los blobs están verificados en IndexedDB.

---

## 6. Descarga y verificación de assets

**`downloadAndCache({ mediaItemId, filePath, checksum })`** en `cache.js`:

1. `fetch(apiUrl + '/' + filePath)`
2. Calcular SHA256 con `SubtleCrypto`:
   ```js
   const hashBuffer = await crypto.subtle.digest('SHA-256', arrayBuffer)
   ```
3. Si `hashHex !== checksum` → `throw Error('Checksum mismatch')`
4. Guardar blob en IndexedDB

> **Nota HTTP:** `SubtleCrypto` solo está disponible en HTTPS o `localhost`.  
> En contexto HTTP → el blob se guarda con `console.warn` sin bloquear reproducción.

---

## 7. Renderer

**`window.SichiRenderer`** en `renderer.js`:

- Un solo `<video id="sichi-player">` en el DOM
- `autoplay`, `muted`, `playsinline` (requeridos por Chrome kiosko)
- Sin `controls`, sin cursor
- Duración por ítem: `overrideDuration ?? duration` (en segundos)
- Al terminar un ítem → `setTimeout` → siguiente ítem
- Al terminar el último → vuelve al primero (loop infinito)
- `URL.revokeObjectURL()` en `stop()` para evitar memory leaks en sesiones 24/7

**API:**
```js
renderer.start()   // carga blobs y arranca carrusel
renderer.stop()    // limpia objectUrls y cancela timer
renderer.reload()  // stop() + start() — usado por forceRefresh
```

---

## 8. SignalR

**Hub:** `/signalr/player`  
**CDN:** `https://unpkg.com/@microsoft/signalr@latest/dist/browser/signalr.min.js`  
**Reconexión automática:** `[0, 2000, 5000, 10000, 30000]` ms

| Evento | Dirección | Acción en player |
|--------|-----------|-----------------|
| `playlistChanged` | Servidor → Cliente | Si `version > localVersion` → sync + reload |
| `forceRefresh` | Servidor → Cliente | sync + reload inmediato |
| `screenConnected` | Cliente → Servidor | Al conectar, envía `screenKey` |

---

## 9. Heartbeat

- **Intervalo:** 30 segundos
- **Endpoint:** `POST /api/player/heartbeat`
- **Body:** `{ screenKey, timestamp: ISO string }`
- Errores de red silenciados — la reproducción nunca se interrumpe por un heartbeat fallido

---

## 10. Pantallas de estado

| Pantalla | Cuándo aparece | Mensaje |
|----------|---------------|---------|
| Waiting | Arranque, aprobación pendiente, sync | "Esperando aprobación..." / "Sincronizando contenido..." |
| Waiting | Offline sin caché | "Sin conexión — reproduciendo desde caché" |
| Error | Sin `?screen=` en URL | "Falta parámetro ?screen=" |
| Video | Reproducción normal | Fullscreen, sin UI |

**Identidad visual pantalla de espera:**
- Fondo: `#121212`
- Logo "SICHI": `#E53935`, 64px, peso 800, letter-spacing 8px
- Subtítulo "DIGITAL SIGNAGE": blanco 50%, 14px, letra 4px
- Mensaje de estado: blanco 70%, 16px, actualizable desde `app.js`

> **Principio:** nunca pantalla negra.

---

## 11. Modo offline

| Escenario | Comportamiento |
|-----------|---------------|
| Sin internet al arrancar | Intenta caché → si existe reproduce, si no → pantalla de espera |
| Pierde conexión durante reproducción | Sigue reproduciendo desde blobs en IndexedDB |
| SignalR se desconecta | Reconexión automática con backoff — reproducción no se interrumpe |
| Heartbeat falla | Warning en consola, reproducción continúa |

---

## 12. Alineación con la API

| Campo API | Nota |
|-----------|------|
| `isApproved` | camelCase — chequeado en register response |
| `mediaItemId` | camelCase — keyPath en IndexedDB |
| `duration` | Ya incluye el override calculado por el backend |
| Checksums | Comparados en minúsculas en ambos lados |
| `screenConnected` | Se envía como string, no como objeto |

---

## 13. Fixes aplicados post-generación

| Fix | Archivo | Descripción |
|-----|---------|-------------|
| Slash en URL de assets | `cache.js` | `apiUrl/filePath` → separador explícito con `.replace` |
| CORS en uploads | `nginx/nginx.conf` | `add_header Access-Control-Allow-Origin *` en bloque `/uploads/` |

---

## 14. Notas de despliegue

- En producción el player se sirve desde `player.sichi.com` vía nginx (Node 4.10)
- No requiere build — archivos estáticos servidos directamente
- Chrome kiosko: agregar flags `--kiosk --noerrdialogs --disable-infobars`
- URL final en TV Box: `http://player.sichi.com/index.html?screen={ScreenKey}`

---

*Documento generado post-commit · Node 4.9 — Player TV · Sichi Digital Signage*
