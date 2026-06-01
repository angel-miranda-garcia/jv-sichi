# Sichi Digital Signage — API Backend
## Documentación Técnica · Nodos 4.2 → 4.5
**Estado:** Completado  
**Fecha:** 2026-06-01  
**Stack:** .NET 10 · Clean Architecture · MySQL 8 · SignalR · EF Core

---

## 1. Estructura del Proyecto

```
api/
├── SichiAPI.sln
├── Domain/
│   ├── Entities/        Screen, Playlist, MediaItem, PlaylistMedia, User, AuditLog
│   ├── Enums/           MediaType (Video)
│   ├── Interfaces/      IScreenRepository, IPlaylistRepository, IMediaRepository,
│   │                    IStorageService, IAuditService, IRealtimeService
│   └── Exceptions/      NotFoundException, ConflictException, ValidationException
├── Application/
│   ├── Auth/            IAuthService, AuthService, LoginResult
│   ├── Screens/         GetApproved, GetPending, Approve, Reject, Update,
│   │                    Delete, ForceRefresh, AssignPlaylist (use cases)
│   ├── Playlists/       Get, GetDetail, Create, Update, Delete,
│   │                    Reorder, AddMedia, RemoveMedia (use cases)
│   ├── Media/           Upload, Delete, GetUsage (use cases)
│   ├── Player/          Register, SyncPlaylist, GetVersion, Heartbeat (use cases)
│   └── Audit/           AuditLogDto
├── Infrastructure/
│   ├── Persistence/     SichiDbContext, ScreenRepository, PlaylistRepository,
│   │                    MediaRepository, AuditService, HeartbeatMonitor
│   ├── Storage/         StorageService, MediaCleanup
│   └── Realtime/        PlayerHub, RealtimeService
└── API/
    ├── Controllers/     Auth, Screens, Playlists, Media, Player, Audit, Health
    ├── Hubs/            PlayerHub (mapeado en Infrastructure)
    └── Middleware/      ErrorHandlingMiddleware, UploadRateLimitMiddleware
```

---

## 2. Modelo de Datos

```
Screen
  Id, Name, ScreenKey(UNIQUE), DeviceId
  CurrentPlaylistId(FK nullable), IsOnline, IsApproved
  LastPing, CreatedAt

Playlist
  Id, Name, Version(default 1), IsActive(default true), CreatedAt

MediaItem
  Id, FileName, FilePath, MediaType(enum Video)
  DurationSeconds, Checksum(SHA256), FileSize, CreatedAt

PlaylistMedia
  PlaylistId(FK), MediaItemId(FK), SortOrder, OverrideDuration(nullable)
  PK compuesta: (PlaylistId, MediaItemId)

User
  Id, Email(UNIQUE), PasswordHash(bcrypt), CreatedAt

AuditLog
  Id, Action, UserId(FK), EntityId, Detail(TEXT), CreatedAt
```

---

## 3. Endpoints

### Auth
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/api/auth/login` | — | Login, retorna JWT (exp 1h) |

### Screens
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/screens` | JWT | Lista pantallas aprobadas |
| GET | `/api/screens/pending` | JWT | Lista pendientes de aprobación |
| GET | `/api/screens/{id}` | JWT | Detalle de pantalla |
| PUT | `/api/screens/{id}` | JWT | Actualiza Name |
| DELETE | `/api/screens/{id}` | JWT | Elimina (409 si tiene playlist) |
| GET | `/api/screens/{id}/status` | JWT | IsOnline, LastPing, CurrentPlaylistId |
| PUT | `/api/screens/{id}/playlist` | JWT | Asigna playlist + emite SignalR |
| POST | `/api/screens/{id}/force-refresh` | JWT | Emite forceRefresh por SignalR |
| POST | `/api/screens/{id}/approve` | JWT | Aprueba pantalla pendiente |
| DELETE | `/api/screens/{id}/reject` | JWT | Rechaza y elimina pendiente |

### Playlists
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/playlists` | JWT | Lista todas |
| GET | `/api/playlists/{id}` | JWT | Detalle con ítems ordenados |
| POST | `/api/playlists` | JWT | Crea playlist |
| PUT | `/api/playlists/{id}` | JWT | Actualiza nombre, incrementa Version |
| DELETE | `/api/playlists/{id}` | JWT | Elimina |
| PUT | `/api/playlists/{id}/reorder` | JWT | Reordena ítems, incrementa Version |
| POST | `/api/playlists/{id}/media` | JWT | Agrega ítem |
| DELETE | `/api/playlists/{id}/media/{mediaId}` | JWT | Elimina ítem |

### Media
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/media` | JWT | Lista todos los archivos |
| POST | `/api/media/upload` | JWT | Upload MP4 (max 50MB, rate limit 10/min) |
| DELETE | `/api/media/{id}` | JWT | Elimina archivo + registro |
| GET | `/api/media/{id}/usage` | JWT | Playlists donde aparece |

### Player (sin JWT)
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/api/player/register` | — | Auto-registro, genera DEVICE-XXXX |
| GET | `/api/player/playlist?screen=xxx` | — | Playlist activa con checksums |
| GET | `/api/player/version?screen=xxx` | — | Versión actual (polling fallback) |
| POST | `/api/player/heartbeat` | — | Actualiza LastPing, IsOnline=true |

### Otros
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | `/api/health` | — | `{ status: "ok", timestamp }` |
| GET | `/api/audit` | JWT | Historial paginado (page, pageSize) |

---

## 4. SignalR Hub

```
Path:    /signalr/player
Grupos:  screen_{screenKey}

Servidor → Cliente:
  playlistChanged   { playlistId, version }   → al asignar playlist
  forceRefresh      {}                         → al llamar force-refresh

Cliente → Servidor:
  screenConnected   { screenKey }              → join al grupo
  heartbeat         { screenKey, timestamp }   → actualiza LastPing en BD
```

---

## 5. Seguridad

| Aspecto | Implementación |
|---------|----------------|
| Admin API | JWT Bearer, expiración 1h, secret desde env var |
| Player API | Sin JWT — autenticación por ScreenKey en query param |
| Upload | Validación MIME real (magic bytes ftyp), solo .mp4 |
| Rate limit | 10 uploads/min por IP (in-memory) |
| Checksum | SHA256 calculado al subir, retornado al player |
| Passwords | bcrypt con BCrypt.Net-Next |

---

## 6. Background Services

| Servicio | Frecuencia | Función |
|----------|-----------|---------|
| `HeartbeatMonitor` | Cada 30s | Marca `IsOnline=false` si `LastPing > 60s` |
| `MediaCleanup` | Cada 24h | Elimina archivos huérfanos en `/uploads/media/` |

---

## 7. StorageService

- Base path: `/app/uploads/media` (env var `UPLOADS_PATH`)
- Estructura: `uploads/media/YYYY/MM/filename_{guid}.mp4`
- Checksum: SHA256 hex lowercase (64 chars)
- Duración: extraída con MediaInfo, fallback `DurationSeconds = 0`

---

## 8. Manejo de Errores

| Excepción | HTTP |
|-----------|------|
| `NotFoundException` | 404 `{ message }` |
| `ConflictException` | 409 `{ message }` |
| `ValidationException` | 400 `{ message }` |
| Otras | 500 `{ message, detail? }` (detail omitido en producción) |

---

## 9. Variables de Entorno Requeridas

```env
DB_CONNECTION=Server=mysql;Port=3306;Database=sichi_db;User=...;Password=...
JWT_SECRET=min-32-chars-secret
ASPNETCORE_ENVIRONMENT=Development|Production
CORS_ORIGINS=http://admin.sichi.com,http://player.sichi.com
ADMIN_EMAIL=admin@sichi.com
ADMIN_PASSWORD=changeme
UPLOADS_PATH=/app/uploads/media
```

---

## 10. Decisiones Técnicas Relevantes

| # | Decisión |
|---|----------|
| Auto-registro | Player hace POST /register → aparece en cola de pendientes |
| DeviceId | Generado como `DEVICE-{4 chars alfanuméricos}` |
| Rechazar dispositivo | Solo elimina de la cola, sin AuditLog |
| OverrideDuration | Duración custom por ítem en playlist (nullable) |
| Playlist offline | Permitido asignar a pantalla offline sin validación |
| Un solo admin | Sin gestión de usuarios ni roles |
| Sin Schedule | No existe tabla ni endpoints de programación |

---

*Documento generado post Node 4.5 · Sichi Digital Signage API*
