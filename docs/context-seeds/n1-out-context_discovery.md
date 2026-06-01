# context_discovery.md
## Sichi Digital Signage — Node 1 Output
## Estado: APPROVED
## Fecha: 2026-05-31
## Próximo nodo: Node 2 — Delivery Planning & Sprint Design

---

## 1. Proyecto

| Campo | Valor |
|---|---|
| Nombre | Sichi Digital Signage |
| Cliente | Jesús V. Cardiel |
| Arquitecto / Desarrollador | Ángel M. García |
| Tipo | Plataforma de señalización digital propia (reemplazo de PosterBooking) |
| Fecha de discovery | 2026-05-31 |

---

## 2. Contexto de Negocio

Restaurante de sushi (Sichi) con una única sucursal activa. El cliente actualmente usa PosterBooking para gestionar contenido en sus pantallas y quiere reemplazarlo con un sistema propio bajo su control total y con identidad de marca Sichi.

**Pantallas:** 3 a 5 TVs en la sucursal.
**Dispositivo de reproducción:** Amazon Firestick — el browser (Firefox o Silk) abre la URL del player en modo kiosco. No hay app nativa. La configuración de pantalla completa / kiosco es tarea de instalación física, no de desarrollo.
**Identificación de pantalla:** La URL incluye un parámetro de pantalla, por ejemplo `https://player.sichi.com?screen=caja`. Se configura una vez por Firestick. No hay flujo de onboarding en el Admin.

---

## 3. Objetivos

- Gestionar contenido de video en 3–5 pantallas desde un Admin web.
- Sincronización automática de playlists vía SignalR cuando el gerente actualiza contenido.
- Operación offline garantizada: nunca pantalla negra.
- Un único administrador gestiona todo el sistema.
- Reemplazar PosterBooking con un sistema propio, simple y con marca Sichi.

---

## 4. Stakeholders

| Rol | Nombre |
|---|---|
| Cliente / Product Owner | Jesús V. Cardiel |
| Arquitecto / Desarrollador único | Ángel M. García |

---

## 5. Decisiones de Negocio Aprobadas

| ID | Decisión | Estado |
|---|---|---|
| D-01 | Una sola sucursal. Sin multitenancy, sin entidad Branch. | APPROVED |
| D-02 | Un único usuario admin en la BD. Sin módulo de gestión de usuarios. Sin roles múltiples. | APPROVED |
| D-03 | Solo video (.mp4 H264). Sin soporte de imágenes. Sin conversión de formatos. | APPROVED |
| D-04 | Sin módulo de programación (Schedule). La playlist asignada a la pantalla es la que se reproduce siempre. | APPROVED |
| D-05 | Sin límite de ítems por playlist. | APPROVED |
| D-06 | Auth simple: un usuario, bcrypt, JWT sin refresh complejo. | APPROVED |
| D-07 | Sin preview de playlist como simulación visual. | APPROVED |
| D-08 | Player como web (HTML/JS) corriendo en browser del Firestick. No es app nativa. No es PWA. | APPROVED |
| D-09 | Admin es web Angular, desktop first. No es PWA. | APPROVED |
| D-10 | Identificación de pantalla por parámetro en URL. Configuración manual una vez por dispositivo. | APPROVED |

---

## 6. Scope Fase 1 — APROBADO

### Dentro del scope

| Módulo | Descripción |
|---|---|
| Auth | Login de un único admin. JWT. Bcrypt. |
| Dashboard | Estado en vivo de pantallas: online/offline, playlist activa, último ping. |
| Pantallas | Listado de TVs, asignar playlist, ver estado, forzar refresh. |
| Playlists | Crear, editar, ordenar ítems con drag & drop. |
| Media | Biblioteca de videos. Subir, eliminar, ver en qué playlists se usa. Solo .mp4 H264, máx 50MB. |
| Player TV | HTML + Vanilla JS. IndexedDB. SignalR. Fallback offline. Solo `<video>`. |
| Historial | AuditLog: quién cambió qué y cuándo. |
| Heartbeat | Ping cada 30 segundos desde cada pantalla. |
| SignalR | Sync automático al actualizar playlist. Fallback polling cada 5 minutos. |

### Fuera del scope Fase 1

- Multitenancy / múltiples sucursales
- Analytics de reproducciones
- App móvil de administración
- App nativa para Firestick
- Programación de contenido por horario o fecha (Schedule)
- Gestión de usuarios (CRUD)
- Soporte de imágenes

---

## 7. Stack Técnico — APPROVED

| Componente | Tecnología |
|---|---|
| Backend | .NET 8 ASP.NET Core Web API |
| Tiempo real | SignalR (self-hosted) |
| Auth | JWT Bearer Token + bcrypt. Un solo usuario. |
| Auth Player | Parámetro `screen` en URL |
| ORM | Entity Framework Core (Code First) |
| Base de datos | MySQL 8 |
| Patrón backend | Clean Architecture ligera |
| Frontend Admin | Angular (última versión estable) |
| Player TV | HTML + CSS + Vanilla JS (sin framework) |
| Caché player | IndexedDB |
| Contenedores | Docker + Docker Compose |
| Servidor | VPS Linux Ubuntu 24.04 (Hostinger KVM 2 o Hetzner CX22) |
| Reverse proxy | Nginx |
| SSL | Let's Encrypt + Certbot |
| Storage | Disco local del VPS — `/uploads/media/YYYY/MM/` |
| IDE de desarrollo | Cursor |

---

## 8. Modelo de Datos — APPROVED (simplificado)

```
Screen
──────────────────────────────────
Id                INT PK AUTO_INCREMENT
Name              VARCHAR(100)        -- "Pantalla Caja"
ScreenKey         VARCHAR(64) UNIQUE  -- coincide con param ?screen=xxx
CurrentPlaylistId INT FK (nullable)
IsOnline          BOOLEAN
LastPing          DATETIME
CreatedAt         DATETIME

Playlist
──────────────────────────────────
Id                INT PK AUTO_INCREMENT
Name              VARCHAR(100)
Version           INT DEFAULT 1       -- sube +1 en cada cambio
IsActive          BOOLEAN
CreatedAt         DATETIME

MediaItem
──────────────────────────────────
Id                INT PK AUTO_INCREMENT
FileName          VARCHAR(255)
FilePath          VARCHAR(500)
MediaType         ENUM('Video')       -- solo video por ahora
DurationSeconds   INT
Checksum          VARCHAR(64)         -- SHA256
FileSize          BIGINT
CreatedAt         DATETIME

PlaylistMedia
──────────────────────────────────
PlaylistId        INT FK
MediaItemId       INT FK
SortOrder         INT
PRIMARY KEY (PlaylistId, MediaItemId)

User
──────────────────────────────────
Id                INT PK AUTO_INCREMENT
Email             VARCHAR(150) UNIQUE
PasswordHash      VARCHAR(255)        -- bcrypt
CreatedAt         DATETIME
-- Sin campo Role: un solo usuario admin

AuditLog
──────────────────────────────────
Id                INT PK AUTO_INCREMENT
Action            VARCHAR(100)        -- "PlaylistUpdated"
UserId            INT FK
EntityId          INT
Detail            TEXT
CreatedAt         DATETIME
```

**Tablas eliminadas respecto al spec original:**
- `Schedule` — eliminada (D-04)

**Campos eliminados:**
- `User.Role` — eliminado (D-02)
- `MediaItem.MediaType ENUM` simplificado a solo `'Video'` (D-03)

---

## 9. Arquitectura de Software

### Clean Architecture Ligera

```
SichiAPI/
│
├── Domain/
│   ├── Entities/        -- Screen, Playlist, MediaItem, User, AuditLog
│   ├── Enums/           -- MediaType
│   └── Interfaces/      -- IPlaylistRepository, IStorageService, IScreenRepository...
│
├── Application/
│   ├── Playlists/       -- GetPlaylistUseCase, UpdatePlaylistUseCase, AssignToScreenUseCase
│   ├── Media/           -- UploadMediaUseCase, DeleteMediaUseCase
│   ├── Screens/         -- GetScreenStatusUseCase, ForceRefreshUseCase
│   └── Player/          -- SyncPlaylistUseCase, HeartbeatUseCase
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

### Regla fundamental de capas
```
Domain      → no depende de nadie
Application → depende solo de Domain
Infrastructure → depende de Domain y Application
API         → depende de todos (solo orquesta)
```

---

## 10. Endpoints Principales — APPROVED

**Auth**
```
POST   /api/auth/login
```

**Screens**
```
GET    /api/screens
GET    /api/screens/{id}
POST   /api/screens
PUT    /api/screens/{id}
DELETE /api/screens/{id}
PUT    /api/screens/{id}/playlist
GET    /api/screens/{id}/status
POST   /api/screens/{id}/force-refresh
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
```

**Player (sin JWT, usa ?screen=key)**
```
GET    /api/player/playlist?screen=xxx
GET    /api/player/version?screen=xxx
POST   /api/player/heartbeat
```

**Health**
```
GET    /api/health
```

**Eliminados respecto al spec original:**
- Todos los endpoints de `/api/schedules`
- `/api/auth/refresh`
- `/api/users`

---

## 11. SignalR Hub — APPROVED

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

## 12. Flujo Principal — Actualizar Contenido

```
1. Subir video
   Admin sube .mp4 desde el Admin.
   Backend valida (H264, máx 50MB), genera checksum SHA256, guarda en /uploads.

2. Editar playlist
   Agrega el ítem, define duración y orden.
   Version de playlist sube +1.

3. Notificación push
   API emite evento SignalR "playlistChanged" a las pantallas asignadas.

4. Sync en TV
   Player descarga los nuevos assets, verifica checksum, guarda en IndexedDB.

5. Renderizado
   Carrusel se actualiza en el siguiente ciclo. Sin pantalla negra.
```

---

## 13. Comportamiento Offline

| Situación | Comportamiento |
|---|---|
| Sin internet | Reproduce última playlist descargada en caché (IndexedDB) |
| SignalR caído | Polling cada 5 min a GET /api/player/version |
| Corte de luz | Firestick arranca solo, browser abre URL en modo kiosco automáticamente |
| Sin playlist asignada | Muestra pantalla de espera con logo de Sichi |
| Pantalla negra | Nunca permitida |

---

## 14. Regla de Oro del Player

> Nunca renderizar antes de tener los assets descargados y verificados.

---

## 15. Requerimientos No Funcionales

| Requerimiento | Detalle |
|---|---|
| Disponibilidad | TV nunca muestra pantalla negra |
| Heartbeat | Ping cada 30 segundos |
| Fallback SignalR | Polling cada 5 minutos |
| Descarga segura | Assets verificados con SHA256 antes de renderizar |
| Rendimiento Admin | Respuesta menor a 2 segundos |
| Escalabilidad | 1 a 20 pantallas sin cambios de arquitectura |

---

## 16. Estilo Visual del Admin

| Aspecto | Definición |
|---|---|
| Paleta | Rojo Sichi (#E53935) + negro + blanco + gris claro |
| Tono | Japonés moderno — clean, tipografía fuerte, íconos claros |
| Responsive | Desktop first |

---

## 17. Costos Estimados

| Concepto | Costo |
|---|---|
| VPS (Hostinger KVM 2 o Hetzner CX22) | ~$7–10 USD/mes |
| Dominio | ~$1 USD/mes |
| SSL (Let's Encrypt) | Gratis |
| **Total** | **~$8–11 USD/mes** |

---

## 18. Riesgos Vigentes

| ID | Riesgo | Probabilidad | Impacto | Mitigación |
|---|---|---|---|---|
| R-01 | Browser del Firestick (Silk/Firefox) con limitaciones en IndexedDB o video playback | Media | Alto | Spike técnico en Node 3.3 con hardware real antes de construir el player |
| R-02 | Assets de video (hasta 50MB × N pantallas) saturan el VPS en picos de sync simultáneo | Baja-Media | Medio | Definir estrategia de sync escalonado en Node 3 |
| R-03 | Scope creep hacia analytics de reproducciones | Alta | Medio | Documentado explícitamente fuera de scope Fase 1 |

---

## 19. Orden de Desarrollo Recomendado

```
1. docker-compose.yml + Dockerfiles
2. Modelo de datos + migraciones EF Core
3. Backend .NET 8 (Auth, Screens, Playlists, Media)
4. SignalR Hub + Player endpoint
5. Player TV (HTML/JS + IndexedDB)
6. Angular Admin
7. Deploy en VPS + Nginx + SSL
```

---

## 20. Gate 1 — APROBADO

| Criterio | Estado |
|---|---|
| Scope preliminar definido | ✅ |
| Objetivos claros | ✅ |
| Stakeholders identificados | ✅ |
| Stack definido y aprobado | ✅ |
| Riesgos identificados | ✅ |
| Modelo de datos simplificado | ✅ |
| Todas las brechas cerradas | ✅ |

**Node 1 CLOSED. Listo para Node 2 — Delivery Planning & Sprint Design.**

---

*Documento generado el 2026-05-31*
*Node 1 — Business Discovery — Sichi Digital Signage*
