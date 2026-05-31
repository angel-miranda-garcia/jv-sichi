# Stack Técnico — Sichi Digital Signage

**Cliente:** Jesús V. Cardiel  
**Proyecto:** Plataforma de Señalización Digital — Restaurante Sichi (Sushi)  
**Fecha:** 31 de mayo de 2026  
**Arquitecto:** Ángel M. García

---

## 1. Infraestructura

| Componente | Detalle |
|---|---|
| Servidor | VPS Linux Ubuntu 24.04 |
| Proveedor recomendado | Hostinger KVM 2 (~$7 USD/mes) o Hetzner CX22 (~$6 USD/mes) |
| CPU | 2 vCPU |
| RAM | 8 GB |
| Almacenamiento | 100 GB NVMe SSD |
| Contenedores | Docker + Docker Compose |

---

## 2. Contenedores (Docker Compose)

| Contenedor | Rol |
|---|---|
| `nginx` | Reverse proxy, HTTPS, sirve estáticos de Angular y Player |
| `api` | .NET 8 API + SignalR Hub |
| `mysql` | Base de datos MySQL 8 |

### Volúmenes persistentes

| Volumen | Descripción |
|---|---|
| `mysql_data` | Datos de la base de datos (nunca se pierden al redeployar) |
| `uploads` | Videos e imágenes subidos (nunca se pierden al redeployar) |

### Puertos

| Puerto | Uso |
|---|---|
| 80 | HTTP → redirige automáticamente a 443 |
| 443 | HTTPS — todo el tráfico público |
| 5000 | API .NET (interno, no expuesto) |
| 3306 | MySQL (interno, no expuesto) |

---

## 3. Backend

| Componente | Tecnología |
|---|---|
| Framework | .NET 8 ASP.NET Core Web API |
| Tiempo real | SignalR (self-hosted) |
| Autenticación Admin | JWT Bearer Token (expiración 1h, refresh 7 días) |
| Autenticación Player | DeviceKey por header `X-Device-Key` |
| ORM | Entity Framework Core |
| Jobs en background | Hosted Services (.NET) |

### Módulos

```
SichiAPI/
├── Controllers/
│   ├── AuthController.cs
│   ├── ScreensController.cs
│   ├── PlaylistsController.cs
│   ├── MediaController.cs
│   ├── PlayerController.cs
│   └── HealthController.cs
│
├── Hubs/
│   └── PlayerHub.cs
│
├── Services/
│   ├── AuthService.cs
│   ├── ScreenService.cs
│   ├── PlaylistService.cs
│   ├── MediaService.cs        -- upload, compress, checksum
│   ├── PlayerService.cs
│   └── ScheduleService.cs     -- resolver qué playlist mostrar
│
├── BackgroundServices/
│   ├── HeartbeatMonitor.cs    -- marca offline si no hay ping
│   └── MediaCleanup.cs        -- borra archivos huérfanos
│
├── Models/
├── DTOs/
├── Data/                      -- DbContext, EF Core
└── Middleware/
    ├── DeviceKeyMiddleware.cs
    └── ErrorHandling.cs
```

### Endpoints principales

**Auth**
```
POST   /api/auth/login
POST   /api/auth/refresh
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
```

**Playlists**
```
GET    /api/playlists
GET    /api/playlists/{id}
POST   /api/playlists
PUT    /api/playlists/{id}
DELETE /api/playlists/{id}
POST   /api/playlists/{id}/media
DELETE /api/playlists/{id}/media/{mediaId}
PUT    /api/playlists/{id}/reorder
```

**Media**
```
GET    /api/media
POST   /api/media/upload
DELETE /api/media/{id}
GET    /api/media/{id}/thumbnail
```

**Player (sin JWT, usa DeviceKey)**
```
GET    /api/player/playlist?deviceKey=xxx
GET    /api/player/version?deviceKey=xxx
POST   /api/player/heartbeat
```

**Schedules**
```
GET    /api/schedules
POST   /api/schedules
PUT    /api/schedules/{id}
DELETE /api/schedules/{id}
```

**Health**
```
GET    /api/health
```

### SignalR Hub

```
Hub:    /signalr/player
Grupos: "screen_{deviceKey}"

Servidor → Cliente:
  playlistChanged   { playlistId, version }
  forceRefresh      {}
  heartbeatAck      { serverTime }

Cliente → Servidor:
  screenConnected   { deviceKey }
  heartbeat         { deviceKey, timestamp }
```

**Fallback si SignalR cae:**
```
Cada 5 minutos → GET /api/player/version
Si versión cambió → sync completo
```

---

## 4. Base de Datos

| Componente | Tecnología |
|---|---|
| Motor | MySQL 8 |
| ORM | Entity Framework Core (Code First) |

### Modelo de datos

```
Screen
──────────────────────────────────
Id                INT PK AUTO_INCREMENT
Name              VARCHAR(100)        -- "Pantalla Caja"
DeviceKey         VARCHAR(64) UNIQUE  -- clave secreta por TV
CurrentPlaylistId INT FK
IsOnline          BOOLEAN
LastPing          DATETIME
CreatedAt         DATETIME

Playlist
──────────────────────────────────
Id                INT PK AUTO_INCREMENT
Name              VARCHAR(100)        -- "Menú Comidas"
Version           INT DEFAULT 1       -- sube +1 en cada cambio
IsActive          BOOLEAN
CreatedAt         DATETIME

MediaItem
──────────────────────────────────
Id                INT PK AUTO_INCREMENT
FileName          VARCHAR(255)
FilePath          VARCHAR(500)        -- /uploads/media/2026/05/file.webp
MediaType         ENUM('Image','Video')
DurationSeconds   INT
Checksum          VARCHAR(64)         -- SHA256 para validar caché
FileSize          BIGINT
CreatedAt         DATETIME

PlaylistMedia
──────────────────────────────────
PlaylistId        INT FK
MediaItemId       INT FK
SortOrder         INT
PRIMARY KEY (PlaylistId, MediaItemId)

Schedule
──────────────────────────────────
Id                INT PK AUTO_INCREMENT
ScreenId          INT FK
PlaylistId        INT FK
Type              ENUM('Horario','Fecha')
DaysOfWeek        VARCHAR(20)         -- "1,2,3,4,5" (solo si Horario)
StartTime         TIME                -- (solo si Horario)
EndTime           TIME                -- (solo si Horario)
StartDate         DATE                -- (solo si Fecha)
EndDate           DATE                -- (solo si Fecha)
IsActive          BOOLEAN
CreatedAt         DATETIME

User
──────────────────────────────────
Id                INT PK AUTO_INCREMENT
Email             VARCHAR(150) UNIQUE
PasswordHash      VARCHAR(255)
Role              ENUM('Admin','Viewer')
CreatedAt         DATETIME

AuditLog
──────────────────────────────────
Id                INT PK AUTO_INCREMENT
Action            VARCHAR(100)        -- "PlaylistUpdated"
UserId            INT FK
EntityId          INT
Detail            TEXT
CreatedAt         DATETIME
```

### Regla de prioridad al resolver playlist activa

```
1. Schedule activo de tipo Fecha que cubra hoy
2. Schedule activo de tipo Horario que cubra la hora actual
3. CurrentPlaylistId de la pantalla (playlist base)
4. Sin ninguna → mostrar pantalla de espera con logo
```

---

## 5. Storage

| Aspecto | Detalle |
|---|---|
| Tipo | Disco local del VPS |
| Ruta | `/uploads/media/YYYY/MM/` |
| Imágenes | .webp (conversión automática al subir) |
| Videos | .mp4 H264 |
| Evitar | .mov, .avi, .mkv (compatibilidad limitada en Android TV) |
| Máx imagen | 2 MB |
| Máx video | 50 MB |

---

## 6. Frontend Admin

| Componente | Tecnología |
|---|---|
| Framework | Angular (última versión estable) |
| Comunicación API | HTTP + SignalR JS Client |
| Autenticación | JWT almacenado en memoria / httpOnly cookie |
| Deploy | Build estático servido por Nginx |
| Acceso | `https://admin.sichi.com` |

### Módulos UI

| Módulo | Rol mínimo |
|---|---|
| Dashboard | Encargado |
| Pantallas | Encargado |
| Playlists | Encargado |
| Media | Encargado |
| Programación | Encargado |
| Usuarios | Solo Gerente |
| Historial | Solo Gerente |

---

## 7. Player TV

| Componente | Tecnología |
|---|---|
| Stack | HTML + CSS + Vanilla JS (sin framework) |
| Tiempo real | SignalR JS Client |
| Caché local | IndexedDB |
| Modo | Chrome en kiosko (fullscreen, sin UI del browser) |
| Acceso | `https://player.sichi.com?deviceKey=xxx` |

### Estructura de archivos

```
player/
├── index.html
├── app.js          -- bootstrap, config
├── signalr.js      -- conexión y eventos
├── playlist.js     -- lógica de sync
├── cache.js        -- download + checksum
├── indexeddb.js    -- persistencia local
├── renderer.js     -- carrusel, transiciones
├── heartbeat.js    -- ping cada 30s
└── styles.css
```

### IndexedDB — stores

| Store | Contenido |
|---|---|
| `config` | deviceKey, url API, última versión conocida |
| `playlist` | datos de la playlist activa |
| `media_blobs` | blobs de imágenes y videos descargados |

### Flujo de arranque

```
Encendido
   │
   ├─→ Cargar config desde IndexedDB
   ├─→ Conectar SignalR
   ├─→ GET /api/player/version
   │
   ├─→ ¿Versión cambió?
   │     SÍ → Descargar assets → Verificar checksum
   │          → Guardar en IndexedDB → Renderizar
   │     NO → Renderizar desde caché
   │
   └─→ Loop de reproducción continua
```

> **Regla de oro:** Nunca renderizar antes de tener los assets descargados y verificados.

---

## 8. Red y Dominio

| Componente | Detalle |
|---|---|
| Dominio | sichi.com (o similar) |
| SSL | Let's Encrypt + Certbot (gratis, auto-renueva) |
| Reverse proxy | Nginx |

### Routing Nginx

```
admin.sichi.com
  /*          → Angular build (estáticos)

player.sichi.com
  /*          → Player HTML/JS (estáticos)

api.sichi.com
  /api/*      → API .NET 8 :5000
  /signalr/*  → SignalR Hub :5000
  /uploads/*  → Archivos media locales
```

---

## 9. Hardware TV

| Componente | Requisito |
|---|---|
| Dispositivo | Android TV Box |
| RAM mínima | 4 GB |
| Almacenamiento | 32 GB mínimo |
| Conexión | WiFi o Ethernet |
| Browser | Chrome (modo kiosko) |
| Auto-boot | Sí — arranca solo tras corte de luz |
| Auto-launch | Sí — abre la URL del player automáticamente |
| Sleep | Desactivado |

---

## 10. Seguridad

| Aspecto | Implementación |
|---|---|
| Admin | JWT Bearer Token |
| Player | DeviceKey por header `X-Device-Key` |
| HTTPS | Obligatorio en producción |
| Validación MIME | Real (no solo por extensión) |
| Rate limit uploads | 10 uploads/min por IP |
| Tamaño máximo imagen | 2 MB |
| Tamaño máximo video | 50 MB |

---

## 11. Costos Estimados

| Concepto | Costo |
|---|---|
| VPS Hostinger KVM 2 o Hetzner CX22 | ~$7–10 USD/mes |
| Dominio | ~$1 USD/mes |
| SSL (Let's Encrypt) | Gratis |
| MySQL | Gratis |
| SignalR | Gratis |
| Storage local | Gratis (incluido en VPS) |
| **Total** | **~$8–11 USD/mes** |

---

## 12. Escalabilidad

| Escenario | Capacidad sin cambios |
|---|---|
| Pantallas por sucursal | 1 a 20 |
| Sucursales | 1 a 10 |
| Usuarios Admin | Sin límite práctico |

---

## 13. Orden de Desarrollo Recomendado

```
1. docker-compose.yml + Dockerfiles
2. Modelo de datos + migraciones EF Core
3. Backend .NET 8 (Auth, Screens, Playlists, Media)
4. SignalR Hub + Player endpoint
5. Player TV (HTML/JS + IndexedDB)
6. Angular Admin
7. Schedule (programación de contenido)
8. Deploy en VPS + configuración Nginx + SSL
```

---

*Documento generado el 31 de mayo de 2026*
