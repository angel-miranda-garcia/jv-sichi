# Node 3 — Estrategia de Diseño Técnico
## Sichi Digital Signage
**Estado:** CLOSED  
**Fecha:** 2026-06-01  
**Próximos nodos:** 4.1 → 4.10

---

## 1. Decisiones Técnicas Cerradas

| ID | Decisión | Fuente |
|----|----------|--------|
| T-01 | Clean Architecture ligera (Domain / Application / Infrastructure / API) | Node 1 |
| T-02 | Identificación de pantalla por `?screen=xxx` en URL. Sin header X-Device-Key. | Node 1 |
| T-03 | Un solo usuario admin. Sin roles. Sin refresh token complejo. JWT 1h. | Node 1 |
| T-04 | Solo video .mp4 H264. Sin imágenes. Sin conversión de formato. | Node 1 |
| T-05 | Sin módulo Schedule. Sin tabla Schedule. | Node 1 |
| T-06 | Auto-registro de pantallas. El player hace primer heartbeat → aparece en cola de pendientes → admin aprueba. Sin creación manual. | Node 3.1 |
| T-07 | Upload directo desde browser (multipart/form-data). Sin URL externa. | Node 3.1 |
| T-08 | Duración por ítem en playlist es editable (campo segundos en PlaylistMedia). No toma solo DurationSeconds del archivo. | Node 3.1 |
| T-09 | Dispositivo rechazado: desaparece de la cola sin log de rechazados. | Node 3.1 |
| T-10 | Asignar playlist a pantalla offline: permitido sin validación de estado. | Node 3.1 |
| T-11 | Validación MIME real en upload (no solo por extensión). | Node 1 |
| T-12 | Rate limit uploads: 10 uploads/min por IP. | Stack técnico |
| T-13 | SHA256 checksum en cada asset — verificado antes de renderizar en Player. | Node 1 |

---

## 2. Modelo de Datos Final

```
Screen
──────────────────────────────────
Id                INT PK AUTO_INCREMENT
Name              VARCHAR(100)        -- "Pantalla Caja"
ScreenKey         VARCHAR(64) UNIQUE  -- coincide con ?screen=xxx
DeviceId          VARCHAR(20)         -- "DEVICE-A3F2" (auto-generado al registrarse)
CurrentPlaylistId INT FK (nullable)
IsOnline          BOOLEAN DEFAULT FALSE
IsApproved        BOOLEAN DEFAULT FALSE  -- FALSE = en cola de pendientes
LastPing          DATETIME
CreatedAt         DATETIME

Playlist
──────────────────────────────────
Id                INT PK AUTO_INCREMENT
Name              VARCHAR(100)
Version           INT DEFAULT 1
IsActive          BOOLEAN DEFAULT TRUE
CreatedAt         DATETIME

MediaItem
──────────────────────────────────
Id                INT PK AUTO_INCREMENT
FileName          VARCHAR(255)
FilePath          VARCHAR(500)        -- /uploads/media/YYYY/MM/
MediaType         ENUM('Video')
DurationSeconds   INT                 -- duración real del archivo
Checksum          VARCHAR(64)         -- SHA256
FileSize          BIGINT
CreatedAt         DATETIME

PlaylistMedia
──────────────────────────────────
PlaylistId        INT FK
MediaItemId       INT FK
SortOrder         INT
OverrideDuration  INT (nullable)      -- duración custom por ítem en playlist
PRIMARY KEY (PlaylistId, MediaItemId)

User
──────────────────────────────────
Id                INT PK AUTO_INCREMENT
Email             VARCHAR(150) UNIQUE
PasswordHash      VARCHAR(255)        -- bcrypt
CreatedAt         DATETIME

AuditLog
──────────────────────────────────
Id                INT PK AUTO_INCREMENT
Action            VARCHAR(100)        -- "PlaylistUpdated", "ScreenApproved"...
UserId            INT FK
EntityId          INT
Detail            TEXT
CreatedAt         DATETIME
```

**Cambios respecto a Node 1:**
- `Screen` agrega `DeviceId` (auto-registro) e `IsApproved` (cola de pendientes)
- `PlaylistMedia` agrega `OverrideDuration` (T-08)
- Eliminado `User.Role` (T-03)
- Eliminada tabla `Schedule` (T-05)

---

## 3. Endpoints Finales

**Auth**
```
POST   /api/auth/login
```

**Screens**
```
GET    /api/screens                        -- lista aprobadas
GET    /api/screens/pending                -- cola de pendientes
GET    /api/screens/{id}
PUT    /api/screens/{id}
DELETE /api/screens/{id}
PUT    /api/screens/{id}/playlist
GET    /api/screens/{id}/status
POST   /api/screens/{id}/force-refresh
POST   /api/screens/{id}/approve
DELETE /api/screens/{id}/reject
```

**Playlists**
```
GET    /api/playlists
GET    /api/playlists/{id}
POST   /api/playlists
PUT    /api/playlists/{id}
DELETE /api/playlists/{id}
PUT    /api/playlists/{id}/reorder
POST   /api/playlists/{id}/media
DELETE /api/playlists/{id}/media/{mediaId}
```

**Media**
```
GET    /api/media
POST   /api/media/upload
DELETE /api/media/{id}
GET    /api/media/{id}/usage              -- en qué playlists aparece
```

**Player (sin JWT, usa ?screen=key)**
```
POST   /api/player/register               -- primer heartbeat, auto-registro
GET    /api/player/playlist?screen=xxx
GET    /api/player/version?screen=xxx
POST   /api/player/heartbeat
```

**Health**
```
GET    /api/health
```

**Eliminados respecto al stack técnico:**
- Todos los endpoints de `/api/schedules`
- `POST /api/auth/refresh`
- `GET /api/media/{id}/thumbnail`
- `POST /api/screens` (reemplazado por auto-registro)

---

## 4. SignalR Hub

```
Hub:    /signalr/player
Grupos: "screen_{screenKey}"

Servidor → Cliente:
  playlistChanged   { playlistId, version }
  forceRefresh      {}

Cliente → Servidor:
  screenConnected   { screenKey }
  heartbeat         { screenKey, timestamp }

Fallback:
  Polling cada 5 min → GET /api/player/version
```

---

## 5. Arquitectura de Software

```
SichiAPI/
│
├── Domain/
│   ├── Entities/        -- Screen, Playlist, MediaItem, PlaylistMedia, User, AuditLog
│   ├── Enums/           -- MediaType
│   └── Interfaces/      -- IPlaylistRepository, IScreenRepository, IMediaRepository,
│                           IStorageService, IAuditService
│
├── Application/
│   ├── Screens/         -- ApproveScreenUseCase, RejectScreenUseCase,
│   │                       GetPendingScreensUseCase, AssignPlaylistUseCase,
│   │                       ForceRefreshUseCase
│   ├── Playlists/       -- GetPlaylistUseCase, CreatePlaylistUseCase,
│   │                       UpdatePlaylistUseCase, ReorderPlaylistUseCase
│   ├── Media/           -- UploadMediaUseCase, DeleteMediaUseCase, GetMediaUsageUseCase
│   └── Player/          -- RegisterScreenUseCase, SyncPlaylistUseCase, HeartbeatUseCase
│
├── Infrastructure/
│   ├── Persistence/     -- EF Core, DbContext, repositorios
│   ├── Storage/         -- disco local, upload, checksum SHA256
│   └── Realtime/        -- SignalR hub
│
└── API/
    ├── Controllers/     -- Auth, Screens, Playlists, Media, Player, Health
    ├── Hubs/            -- PlayerHub
    └── Middleware/      -- ErrorHandling
```

---

## 6. Infraestructura y Docker

```yaml
# Contenedores
nginx    -- reverse proxy, HTTPS, sirve estáticos Angular y Player
api      -- .NET 8 API + SignalR Hub (puerto interno 5000)
mysql    -- MySQL 8 (puerto interno 3306)

# Volúmenes persistentes
mysql_data  -- datos BD
uploads     -- videos subidos

# Puertos públicos
80   → redirect a 443
443  → todo el tráfico público

# Routing Nginx
admin.sichi.com   → Angular build (estáticos)
player.sichi.com  → Player HTML/JS (estáticos)
api.sichi.com/api/*      → API :5000
api.sichi.com/signalr/*  → SignalR Hub :5000
api.sichi.com/uploads/*  → archivos media locales
```

---

## 7. Background Services

| Servicio | Función |
|----------|---------|
| `HeartbeatMonitor` | Marca `IsOnline = false` si `LastPing` > 60 segundos |
| `MediaCleanup` | Borra archivos en disco que ya no tienen registro en BD |

---

## 8. Seguridad

| Aspecto | Implementación |
|---------|----------------|
| Admin | JWT Bearer Token, expiración 1h |
| Player | `?screen=xxx` en URL (ScreenKey) |
| HTTPS | Obligatorio en producción |
| Validación MIME | Real, no solo por extensión |
| Rate limit uploads | 10 uploads/min por IP |
| Tamaño máximo video | 50 MB |
| Checksum | SHA256 verificado en Player antes de renderizar |

---

## 9. Comportamiento Offline del Player

| Situación | Comportamiento |
|-----------|----------------|
| Sin internet | Reproduce última playlist en caché (IndexedDB) |
| SignalR caído | Polling cada 5 min a `/api/player/version` |
| Corte de luz | Android TV Box arranca solo, Chrome abre URL automáticamente |
| Sin playlist asignada | Pantalla de espera con logo Sichi |
| Pantalla negra | Nunca permitida |

**Regla de oro:** Nunca renderizar antes de tener los assets descargados y verificados.

---

## 10. Identidad Visual (referencia para nodos 4.6–4.8)

| Token | Valor |
|-------|-------|
| `--sichi-red` | `#E53935` |
| `--sichi-black` | `#121212` |
| `--sichi-gray` | `#F5F5F5` |
| `--text-muted` | `#666666` |
| `--border-color` | `#E0E0E0` |
| `--status-online` | `#4CAF50` |
| `--status-offline` | `#F44336` |
| `--border-radius` | `4px` |
| Font | Inter (Google Fonts), pesos 400/500/600/700/800 |

Prototipo HTML navegable aprobado por cliente en Node 3.1 — usar como referencia visual exacta.

---

## 11. Mapa de Nodos 4x

| Nodo | Nombre | Depende de | Bloquea |
|------|--------|-----------|---------|
| 4.1 | Infraestructura Base | — | 4.2 |
| 4.2 | Backend: Fundación | 4.1 | 4.3, 4.4, 4.5 |
| 4.3 | Backend: Auth + Screens + Playlists | 4.2 | 4.6, 4.7 |
| 4.4 | Backend: Media | 4.2 | 4.8 |
| 4.5 | Backend: Player + SignalR | 4.2 | 4.9 |
| 4.6 | Angular Admin: Scaffolding + Auth | 4.3, Node 3.1 | 4.7, 4.8 |
| 4.7 | Angular Admin: Screens + Playlists | 4.6 | — |
| 4.8 | Angular Admin: Media + Dashboard | 4.6 | — |
| 4.9 | Player TV | 4.5 | — |
| 4.10 | Deploy | 4.3–4.9 | — |

---

## 12. Seeds para Nodos 4x

---

### Seed Node 4.1 — Infraestructura Base

**Objetivo:** Tener el entorno Docker corriendo localmente con nginx, api y mysql comunicados.

**Output esperado de Cursor:**
- `docker-compose.yml`
- `Dockerfile` para API .NET 8
- `nginx/nginx.conf` con routing por subdominio
- `.env.example` con todas las variables requeridas
- `README-deploy.md` con instrucciones de primer arranque

**Prompt para Cursor:**
```
Proyecto: Sichi Digital Signage
Stack: .NET 8 API + MySQL 8 + Nginx en Docker Compose

Crea la infraestructura base:

1. docker-compose.yml con 3 servicios:
   - nginx (reverse proxy, puertos 80 y 443)
   - api (.NET 8, puerto interno 5000, depende de mysql)
   - mysql (MySQL 8, puerto interno 3306, volumen persistente mysql_data)
   - Volumen adicional: uploads (compartido entre api y nginx)

2. Dockerfile para el servicio api:
   - Base: mcr.microsoft.com/dotnet/aspnet:8.0
   - Build: mcr.microsoft.com/dotnet/sdk:8.0
   - Multi-stage build
   - Expone puerto 5000

3. nginx/nginx.conf:
   - admin.sichi.com → /var/www/admin (estáticos)
   - player.sichi.com → /var/www/player (estáticos)
   - api.sichi.com/api/* → proxy_pass http://api:5000
   - api.sichi.com/signalr/* → proxy_pass http://api:5000 con WebSocket upgrade
   - api.sichi.com/uploads/* → alias al volumen uploads
   - Puerto 80 redirige a 443 (placeholder SSL con certificado self-signed para dev)

4. .env.example con variables:
   MYSQL_ROOT_PASSWORD, MYSQL_DATABASE, MYSQL_USER, MYSQL_PASSWORD,
   JWT_SECRET, ASPNETCORE_ENVIRONMENT, CORS_ORIGINS

5. README-deploy.md: pasos para `docker compose up` en desarrollo

Commit: feat(infra): docker compose base con nginx, api y mysql
```

---

### Seed Node 4.2 — Backend: Fundación

**Objetivo:** Proyecto .NET 8 con Clean Architecture, EF Core conectado a MySQL, migraciones iniciales corriendo.

**Output esperado de Cursor:**
- Estructura de carpetas completa (Domain / Application / Infrastructure / API)
- Todas las entidades del modelo de datos
- DbContext con configuraciones EF Core
- Migración inicial
- Health endpoint respondiendo

**Prompt para Cursor:**
```
Proyecto: Sichi Digital Signage — Backend .NET 8
Patrón: Clean Architecture ligera

Crea el proyecto SichiAPI con esta estructura:

SichiAPI/
├── Domain/
│   ├── Entities/   (Screen, Playlist, MediaItem, PlaylistMedia, User, AuditLog)
│   ├── Enums/      (MediaType: Video)
│   └── Interfaces/ (IScreenRepository, IPlaylistRepository, IMediaRepository,
│                    IStorageService, IAuditService)
├── Application/
│   ├── Screens/
│   ├── Playlists/
│   ├── Media/
│   └── Player/
├── Infrastructure/
│   ├── Persistence/  (SichiDbContext, repositorios EF Core)
│   ├── Storage/
│   └── Realtime/
└── API/
    ├── Controllers/  (HealthController únicamente en este nodo)
    ├── Hubs/
    └── Middleware/   (ErrorHandlingMiddleware global)

Entidades exactas:

Screen: Id, Name, ScreenKey(unique), DeviceId, CurrentPlaylistId(FK nullable),
        IsOnline(bool), IsApproved(bool default false), LastPing, CreatedAt

Playlist: Id, Name, Version(int default 1), IsActive(bool), CreatedAt

MediaItem: Id, FileName, FilePath, MediaType(enum Video), DurationSeconds,
           Checksum(SHA256), FileSize, CreatedAt

PlaylistMedia: PlaylistId(FK), MediaItemId(FK), SortOrder, OverrideDuration(nullable)
               PK compuesta (PlaylistId, MediaItemId)

User: Id, Email(unique), PasswordHash, CreatedAt

AuditLog: Id, Action, UserId(FK), EntityId, Detail(text), CreatedAt

Configuración:
- EF Core Code First con MySQL 8 (Pomelo.EntityFrameworkCore.MySql)
- Connection string desde variable de entorno
- Migración inicial: InitialCreate
- GET /api/health retorna { status: "ok", timestamp: utcNow }

Commit: feat(backend): clean architecture scaffolding + EF Core + migración inicial
```

---

### Seed Node 4.3 — Backend: Auth + Screens + Playlists

**Objetivo:** Auth JWT funcional, CRUD de screens con flujo de aprobación, CRUD de playlists con reorder.

**Prompt para Cursor:**
```
Proyecto: Sichi Digital Signage — Backend Node 4.3
Base: proyecto del Node 4.2 ya existente

Implementa los siguientes módulos:

AUTH:
- POST /api/auth/login → recibe { email, password }, retorna JWT (exp 1h)
- bcrypt para verificar password
- Un solo usuario admin (seed en DbContext si no existe)
- JWT secret desde variable de entorno

SCREENS (todos requieren JWT excepto donde se indique):
- GET  /api/screens              → lista screens donde IsApproved = true
- GET  /api/screens/pending      → lista screens donde IsApproved = false
- GET  /api/screens/{id}
- PUT  /api/screens/{id}         → actualiza Name
- DELETE /api/screens/{id}
- PUT  /api/screens/{id}/playlist → asigna CurrentPlaylistId, incrementa Version de playlist,
                                    emite SignalR playlistChanged
- GET  /api/screens/{id}/status  → IsOnline, LastPing, CurrentPlaylistId
- POST /api/screens/{id}/force-refresh → emite SignalR forceRefresh al grupo screen_{screenKey}
- POST /api/screens/{id}/approve → IsApproved = true, registra AuditLog
- DELETE /api/screens/{id}/reject → elimina registro, sin AuditLog

PLAYLISTS (todos requieren JWT):
- GET    /api/playlists
- GET    /api/playlists/{id}     → incluye PlaylistMedia ordenado por SortOrder
- POST   /api/playlists          → crea playlist, Version = 1
- PUT    /api/playlists/{id}     → actualiza Name, incrementa Version, registra AuditLog
- DELETE /api/playlists/{id}
- PUT    /api/playlists/{id}/reorder → recibe array [{ mediaItemId, sortOrder }],
                                       actualiza SortOrder, incrementa Version
- POST   /api/playlists/{id}/media   → agrega MediaItem a playlist con SortOrder y
                                        OverrideDuration opcional
- DELETE /api/playlists/{id}/media/{mediaId} → elimina ítem de playlist

AuditLog: registrar en: login, screen approve, playlist create/update/delete

Commit: feat(backend): auth JWT + screens approval flow + playlists CRUD
```

---

### Seed Node 4.4 — Backend: Media

**Objetivo:** Upload de videos con validación real, checksum, cleanup automático.

**Prompt para Cursor:**
```
Proyecto: Sichi Digital Signage — Backend Node 4.4
Base: proyecto Node 4.3

Implementa el módulo Media:

ENDPOINTS (todos requieren JWT):
- GET  /api/media          → lista todos los MediaItems
- POST /api/media/upload   → sube archivo, retorna MediaItem creado
- DELETE /api/media/{id}   → elimina registro + archivo en disco
- GET  /api/media/{id}/usage → retorna lista de playlists donde aparece el ítem

VALIDACIONES en upload:
- Solo .mp4 con codec H264 (validar MIME type real, no solo extensión)
- Tamaño máximo: 50 MB
- Rate limit: 10 uploads/min por IP (usar middleware o atributo)
- Calcular SHA256 checksum del archivo
- Extraer DurationSeconds del archivo (usar MediaInfo o similar)
- Guardar en /uploads/media/YYYY/MM/filename_uuid.mp4

STORAGE SERVICE (IStorageService implementado en Infrastructure/Storage):
- SaveFileAsync(stream, filename) → retorna FilePath
- DeleteFileAsync(filePath)
- GetChecksumAsync(filePath) → SHA256

BACKGROUND SERVICE HeartbeatMonitor:
- Hosted Service que corre cada 30 segundos
- Busca screens donde LastPing < utcNow - 60 segundos y IsOnline = true
- Los marca IsOnline = false

BACKGROUND SERVICE MediaCleanup:
- Hosted Service que corre cada 24 horas
- Busca archivos en /uploads/media/ que no tienen registro en BD
- Los elimina del disco

Commit: feat(backend): media upload + storage service + background services
```

---

### Seed Node 4.5 — Backend: Player + SignalR

**Objetivo:** Endpoints del player, auto-registro, SignalR hub funcional.

**Prompt para Cursor:**
```
Proyecto: Sichi Digital Signage — Backend Node 4.5
Base: proyecto Node 4.4

Implementa el módulo Player y SignalR:

SIGNALR HUB (Infrastructure/Realtime/PlayerHub.cs):
Hub path: /signalr/player
Grupos: "screen_{screenKey}"

Cliente → Servidor:
- screenConnected({ screenKey }) → agrega conexión al grupo screen_{screenKey}
- heartbeat({ screenKey, timestamp }) → actualiza LastPing e IsOnline = true en BD

Servidor → Cliente (métodos para llamar desde controllers):
- playlistChanged({ playlistId, version }) → al grupo específico
- forceRefresh({}) → al grupo específico

ENDPOINTS PLAYER (sin JWT, autenticación por ScreenKey en query param ?screen=xxx):

POST /api/player/register
  Body: { screenKey, deviceId? }
  - Si screenKey no existe: crea Screen con IsApproved = false, DeviceId auto-generado
    formato DEVICE-{4 chars alfanuméricos aleatorios}
  - Si ya existe: retorna datos actuales
  - Retorna: { deviceId, isApproved, message }

GET /api/player/playlist?screen=xxx
  - Valida que screen exista y IsApproved = true
  - Retorna playlist activa con ítems ordenados por SortOrder
  - Cada ítem incluye: mediaItemId, filePath, checksum, duration (OverrideDuration ?? DurationSeconds)

GET /api/player/version?screen=xxx
  - Retorna: { version, playlistId }
  - Para polling de fallback

POST /api/player/heartbeat
  Body: { screenKey, timestamp }
  - Actualiza LastPing, IsOnline = true

CORS: permitir origen player.sichi.com + localhost para desarrollo

Commit: feat(backend): player endpoints + auto-register + SignalR hub
```

---

### Seed Node 4.6 — Angular Admin: Scaffolding + Auth

**Objetivo:** Proyecto Angular con layout base, paleta Sichi, login funcional contra el API.

**Prompt para Cursor:**
```
Proyecto: Sichi Digital Signage — Angular Admin Node 4.6
Referencia visual: prototipo HTML aprobado en Node 3.1

Crea el proyecto Angular (última versión estable) con:

ESTRUCTURA:
src/app/
├── core/
│   ├── auth/          (AuthService, AuthGuard, JwtInterceptor)
│   ├── models/        (Screen, Playlist, MediaItem, AuditLog interfaces)
│   └── services/      (ScreenService, PlaylistService, MediaService, AuditService)
├── layout/
│   └── sidebar/       (componente sidebar colapsable)
├── features/
│   ├── login/
│   ├── dashboard/
│   ├── screens/
│   ├── playlists/
│   ├── media/
│   └── history/
└── shared/
    └── components/    (toast, badge, modal-base)

VARIABLES CSS GLOBALES (styles.scss):
--sichi-red: #E53935
--sichi-black: #121212
--sichi-gray: #F5F5F5
--text-muted: #666666
--border-color: #E0E0E0
--status-online: #4CAF50
--status-offline: #F44336
--border-radius: 4px
Font: Inter desde Google Fonts

LAYOUT:
- Sidebar izquierdo fijo (260px expandido / 80px colapsado)
- Colapsado: oculta textos, centra íconos
- Header con título dinámico + chip "Admin activo"
- Área de contenido scrolleable

LOGIN:
- Fondo --sichi-black
- Card centrada max-width 380px
- Logo SICHI en rojo, subtítulo DIGITAL SIGNAGE
- Form: email + password
- Submit → POST /api/auth/login
- JWT guardado en memory (no localStorage)
- Redirect a /dashboard en éxito
- Error state inline en bloque rojo claro

GUARDS:
- AuthGuard: redirige a /login si no hay JWT
- JWT interceptor: agrega Bearer token a todas las peticiones API

TOAST SERVICE:
- Fixed bottom-right
- Tipos: success (borde verde) / error (borde rojo)
- Auto-dismiss 3000ms + fade-out 300ms
- Apilable

Commit: feat(admin): angular scaffolding + layout + auth + toast service
```

---

### Seed Node 4.7 — Angular Admin: Screens + Playlists

**Objetivo:** Módulos de pantallas (con tabs y flujo de aprobación) y playlists (con reorder drag & drop).

**Prompt para Cursor:**
```
Proyecto: Sichi Digital Signage — Angular Admin Node 4.7
Base: proyecto Node 4.6

MÓDULO SCREENS:
Ruta: /screens

Tab "Activas" (IsApproved = true):
- Tabla: Nombre/Ubicación · MAC · Estado (pill online/offline) · Playlist Asignada ·
         Último Ping · Acciones
- Acción "Asignar Playlist" → modal con dropdown de playlists disponibles
- Acción "Forzar Refresh" → POST /api/screens/{id}/force-refresh + toast
- Acción "Eliminar" → modal confirm → DELETE /api/screens/{id}

Tab "Pendientes" (IsApproved = false):
- Badge rojo con número en sidebar nav si hay pendientes
- Tabla: Device ID · MAC · Primera Conexión · Acciones
- Acción "Aprobar" → POST /api/screens/{id}/approve + pasa a tab Activas + toast
- Acción "Rechazar" → DELETE /api/screens/{id}/reject + desaparece + toast
- Empty state: icono monitor + "No hay dispositivos pendientes de aprobación"

Estado online en tiempo real: SignalR client actualiza pill sin recargar página.

MÓDULO PLAYLISTS:
Ruta: /playlists

Vista Listado:
- Tabla: Nombre · Ítems · Duración Total · Última Modificación · Acciones
- Acciones: Editar → /playlists/{id}/edit · Eliminar → modal confirm
- Botón "+ Crear Playlist" → /playlists/new

Vista Editar/Crear (ruta: /playlists/:id/edit o /playlists/new):
- Header sticky: input nombre inline + botones Cancelar / Guardar
- Lista de ítems con:
  - Drag handle (CDK DragDrop de Angular para reorder funcional)
  - Thumbnail placeholder
  - Nombre del video
  - Input segundos (OverrideDuration)
  - Botón eliminar ítem (inmediato, sin confirm)
- Botón "+ Agregar Ítem" → abre modal selector de media
  - Grid de MediaItems disponibles
  - Click en uno → lo agrega al final de la lista
- Guardar → PUT /api/playlists/{id} + PUT /api/playlists/{id}/reorder si cambió orden

Commit: feat(admin): screens approval flow + playlists editor con drag & drop
```

---

### Seed Node 4.8 — Angular Admin: Media + Dashboard

**Objetivo:** Biblioteca de media con upload, y dashboard con estado live por SignalR.

**Prompt para Cursor:**
```
Proyecto: Sichi Digital Signage — Angular Admin Node 4.8
Base: proyecto Node 4.7

MÓDULO MEDIA:
Ruta: /media

Upload zone:
- Drop zone con texto "Haz clic o arrastra archivos multimedia aquí"
- Sub-texto: "Solo .mp4 H264 · Máx 50MB"
- Validación cliente: tipo y tamaño antes de enviar
- Progress bar durante upload (HttpClient con reportProgress)
- Toast éxito/error al terminar

Grid biblioteca (auto-fill min 220px):
- Card por ítem: thumbnail color (video → rojo) · nombre · tipo + peso + duración · acciones
- Acción "Ver uso" → modal con lista de playlists donde aparece
  (GET /api/media/{id}/usage)
  Empty: texto italic "No se está utilizando en ninguna playlist"
- Acción "Eliminar" → modal confirm con advertencia si está en uso

MÓDULO DASHBOARD:
Ruta: /dashboard (default tras login)

Métricas (grid 3 cols):
- Total Pantallas (negro)
- Online (verde --status-online)
- Offline (rojo --status-offline)

Tabla "Monitoreo en vivo":
- Columnas: Ubicación · Estado · Playlist Activa · Último Ping · Acción Rápida
- Pill online/offline actualizado en tiempo real por SignalR
- Botón "Forzar Refresh": deshabilitado (opacity 0.5) si IsOnline = false

SignalR en Admin:
- Conectar a /signalr/player al iniciar sesión
- Escuchar eventos para actualizar estado de pantallas en tiempo real
- Desconectar al hacer logout

MÓDULO HISTORIAL:
Ruta: /history
- Tabla AuditLog: Fecha/Hora · Usuario · Acción · Entidad · Detalle
- Paginación: 10 registros/página
- Info "Mostrando X - Y de N registros"
- Filtro por tipo de acción (dropdown)

Commit: feat(admin): media upload + dashboard live + historial paginado
```

---

### Seed Node 4.9 — Player TV

**Objetivo:** Player HTML/JS corriendo en Chrome kiosko con IndexedDB, sync, heartbeat y fallback offline.

**Prompt para Cursor:**
```
Proyecto: Sichi Digital Signage — Player TV Node 4.9
Stack: HTML + CSS + Vanilla JS (sin framework)
Entorno: Chrome en modo kiosko en Android TV Box

Crea la carpeta player/ con estos archivos:

player/
├── index.html
├── styles.css
├── app.js          -- bootstrap, lee ?screen=xxx de URL, init
├── signalr.js      -- conexión SignalR, eventos playlistChanged y forceRefresh
├── playlist.js     -- lógica de sync: detectar versión nueva, orquestar descarga
├── cache.js        -- download asset, verificar SHA256, guardar blob
├── indexeddb.js    -- init DB, get/set config, playlist y media_blobs
├── renderer.js     -- carrusel fullscreen, transiciones, loop infinito
└── heartbeat.js    -- ping cada 30s a POST /api/player/heartbeat

IndexedDB stores:
- config: { screenKey, apiUrl, lastKnownVersion }
- playlist: { id, version, items: [{ mediaItemId, filePath, checksum, duration }] }
- media_blobs: { mediaItemId, blob, checksum }

FLUJO DE ARRANQUE (app.js):
1. Leer screenKey de ?screen=xxx en URL
2. Cargar config desde IndexedDB
3. POST /api/player/register → si IsApproved = false → mostrar pantalla de espera
4. Conectar SignalR → screenConnected({ screenKey })
5. GET /api/player/version → comparar con lastKnownVersion en IndexedDB
6. Si versión cambió → playlist.sync()
7. Si versión igual → renderer.start() desde caché
8. Iniciar heartbeat.js

SYNC (playlist.js):
1. GET /api/player/playlist?screen=xxx
2. Para cada ítem: si blob existe en IndexedDB y checksum coincide → skip
3. Si no → cache.download(filePath, checksum)
4. Cuando todos los ítems están verificados → guardar playlist en IndexedDB
5. Actualizar lastKnownVersion en config
6. renderer.start()

REGLA DE ORO: nunca llamar renderer.start() sin que todos los blobs estén
verificados en IndexedDB.

RENDERER (renderer.js):
- Fullscreen negro
- Un elemento <video> que itera sobre los ítems en orden SortOrder
- Al terminar la duración del ítem → siguiente ítem
- Al terminar el último → volver al primero (loop infinito)
- Sin controles del video, sin cursor

OFFLINE:
- Sin internet al arrancar → renderer.start() desde caché si existe
- Si caché vacía → mostrar pantalla de espera (logo Sichi, fondo negro)
- Nunca pantalla negra

PANTALLA DE ESPERA:
- Fondo #121212
- Logo "SICHI" centrado en #E53935
- Texto "Cargando contenido..." en blanco

Commit: feat(player): HTML/JS player con IndexedDB, sync, heartbeat y fallback offline
```

---

### Seed Node 4.10 — Deploy

**Objetivo:** Sistema corriendo en VPS de producción con HTTPS real.

**Prompt para Cursor:**
```
Proyecto: Sichi Digital Signage — Deploy Node 4.10
Servidor: VPS Ubuntu 24.04 (Hostinger KVM 2 o Hetzner CX22)

Genera los scripts y configuraciones de producción:

1. nginx/nginx.prod.conf:
   - Certificados SSL reales: /etc/letsencrypt/live/sichi.com/
   - admin.sichi.com, player.sichi.com, api.sichi.com
   - HSTS headers
   - Gzip habilitado
   - Proxy headers correctos para SignalR WebSocket

2. docker-compose.prod.yml:
   - Sobrescribe puertos y variables para producción
   - Restart: always en todos los servicios
   - Logging con límite de tamaño

3. scripts/deploy.sh:
   - git pull
   - docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
   - docker compose exec api dotnet ef database update

4. scripts/setup-ssl.sh:
   - Instala certbot
   - Obtiene certificados para admin.sichi.com, player.sichi.com, api.sichi.com
   - Configura auto-renovación con cron

5. scripts/backup-db.sh:
   - mysqldump dentro del contenedor mysql
   - Guarda en /backups/YYYY-MM-DD_sichi.sql.gz
   - Retiene últimos 7 días

6. DEPLOY.md:
   - Checklist completo de primer deploy
   - Variables de entorno de producción requeridas
   - Smoke test: lista de URLs a verificar manualmente

Commit: feat(deploy): producción nginx SSL + scripts deploy + backup DB
```

---

## 13. Riesgos Vigentes

| ID | Riesgo | Mitigación |
|----|--------|-----------|
| R-01 | Chrome en Android TV Box con limitaciones en IndexedDB o video playback | Spike técnico con hardware real antes o durante Node 4.9 |
| R-02 | Assets de video hasta 50MB saturan VPS en sync simultáneo de todas las pantallas | HeartbeatMonitor escalonado; revisar en Node 4.5 |
| R-03 | Scope creep hacia analytics o scheduling | Documentado fuera de scope. Cualquier adición es Node 5+ |

---

## 14. Gate 3 — CLOSED

| Criterio | Estado |
|----------|--------|
| Decisiones técnicas sin ambigüedad | ✅ |
| Modelo de datos final con cambios de Node 3.1 | ✅ |
| Endpoints finales definidos | ✅ |
| Flujo de auto-registro de pantallas definido | ✅ |
| Duración override por ítem en playlist definida | ✅ |
| Seeds para los 10 nodos ejecutores generados | ✅ |
| Prototipo HTML aprobado por cliente (Node 3.1) | ✅ |
| Mapa de dependencias entre nodos claro | ✅ |

**Node 3 CLOSED. Listo para ejecutar Node 4.1 → 4.10 en Cursor.**

---

*Documento generado el 2026-06-01*  
*Node 3 — Technical Design Strategy — Sichi Digital Signage*

## 15. Entorno de Desarrollo — Versiones Confirmadas

| Herramienta | Versión | Notas |
|-------------|---------|-------|
| .NET SDK | 10.0.300 | Actualizar Dockerfiles: aspnet:10.0 y sdk:10.0 |
| Node.js | 24.16.0 | |
| npm | 11.13.0 | |
| Docker | 29.4.3 | |
| Angular CLI | por instalar en Node 4.6 | |
| SO desarrollo | Windows | |

**Nota .NET 10:** Los seeds de Node 4.1 y 4.2 usan imágenes :8.0 en los Dockerfiles.
Reemplazar por :10.0 al ejecutar en Cursor.

## 16. Estructura del Repositorio

Monorepo único: `jv-sichi`
jv-sichi/
├── api/                -- .NET 10 backend (Clean Architecture)
├── admin/              -- Angular Admin
├── player/             -- Player TV (HTML/JS Vanilla)
├── nginx/              -- nginx.conf + nginx.prod.conf
├── scripts/            -- deploy.sh, backup-db.sh, setup-ssl.sh
├── docker-compose.yml
├── docker-compose.prod.yml
└── .env.example