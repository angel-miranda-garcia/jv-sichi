# delivery_plan.md
## Sichi Digital Signage — Node 2 Output (Expandido con Tasks)
## Estado: APPROVED
## Fecha: 2026-05-31
## Próximo nodo: Node 3 — Technical Architecture & Engineering Strategy

---

## 1. Resumen de Entrega

| Campo | Valor |
|---|---|
| Proyecto | Sichi Digital Signage |
| Alcance | Fase 1 completa (según Node 1) |
| Modelo de entrega | Sprint único de 5 días |
| Desarrollador | Ángel M. García (solo) |

---

## 2. Epic

**EPIC-01 — Sistema de Señalización Digital Sichi**

> Como administrador del restaurante Sichi, quiero gestionar el contenido en video de las pantallas del local desde un panel web, para reemplazar PosterBooking con un sistema propio que siempre esté activo y bajo mi control.

---

## 3. Historias de Usuario con Tasks Detallados

---

### Auth

---

#### HU-01 — Login de administrador

> Como administrador, quiero iniciar sesión con usuario y contraseña para acceder al panel de gestión.

**Criterios de aceptación:**
- Formulario de login con usuario y contraseña
- JWT devuelto en respuesta, almacenado en cliente
- Contraseña hasheada con bcrypt en la BD
- Redirige al dashboard tras login exitoso
- Muestra error genérico en credenciales inválidas

**Tasks — Backend:**

| ID | Task | Detalle |
|---|---|---|
| T-01.BE.01 | Crear entidad `User` y migración EF Core | Campos: Id, Email, PasswordHash, CreatedAt. Sin campo Role. |
| T-01.BE.02 | Implementar `POST /api/auth/login` | Recibe `{ email, password }`. Valida contra BD. Retorna `{ token, expiresAt }`. |
| T-01.BE.03 | Configurar JWT Bearer en `Program.cs` | Secret desde `appsettings` / env var. Expiración: 8 horas. |
| T-01.BE.04 | Hashear contraseña con bcrypt | Usar `BCrypt.Net-Next`. Hash al seed del usuario admin. Verificar en login. |
| T-01.BE.05 | Seed del usuario admin en `DbContext` | Un único usuario. Email y password configurables por env var. Ejecutar en migration. |
| T-01.BE.06 | Respuesta de error genérica en credenciales inválidas | HTTP 401, body `{ message: "Credenciales inválidas" }`. No revelar si el email existe. |

**Tasks — Frontend:**

| ID | Task | Detalle |
|---|---|---|
| T-01.FE.01 | Crear componente `LoginComponent` | Formulario reactivo Angular con campos email y password. |
| T-01.FE.02 | Implementar `AuthService` | Método `login(email, password)`. Llama a `POST /api/auth/login`. Almacena JWT en `localStorage`. |
| T-01.FE.03 | Implementar `AuthGuard` | Redirige a `/login` si no hay JWT válido. Aplica a todas las rutas del Admin. |
| T-01.FE.04 | Redirigir al Dashboard tras login exitoso | `Router.navigate(['/dashboard'])` tras JWT recibido. |
| T-01.FE.05 | Mostrar mensaje de error en credenciales inválidas | Mensaje debajo del formulario. No exponer detalle técnico. |
| T-01.FE.06 | Interceptor HTTP para adjuntar JWT | `HttpInterceptor` que agrega `Authorization: Bearer {token}` en cada request. |

---

### Dashboard

---

#### HU-02 — Estado en vivo de pantallas

> Como administrador, quiero ver en el dashboard el estado actual de cada pantalla para saber cuáles están activas y qué contenido están reproduciendo.

**Criterios de aceptación:**
- Lista de pantallas con: nombre, estado online/offline, playlist activa, último ping
- Estado calculado: online si último ping < 60 segundos
- Actualización en tiempo real vía SignalR
- Sin acciones desde el dashboard (solo lectura)

**Tasks — Backend:**

| ID | Task | Detalle |
|---|---|---|
| T-02.BE.01 | Implementar `GET /api/screens` | Retorna lista con: Id, Name, IsOnline, CurrentPlaylistName, LastPing. |
| T-02.BE.02 | Lógica de cálculo `IsOnline` | Online si `LastPing` dentro de los últimos 60 segundos. Calculado en query, no almacenado. |
| T-02.BE.03 | Emitir evento SignalR `screenStatusUpdated` desde heartbeat | Al recibir ping, emitir estado actualizado al grupo `admin` para actualizar dashboard. |

**Tasks — Frontend:**

| ID | Task | Detalle |
|---|---|---|
| T-02.FE.01 | Crear componente `DashboardComponent` | Vista principal post-login. |
| T-02.FE.02 | Crear componente `ScreenStatusCardComponent` | Tarjeta por pantalla: nombre, badge online/offline (color), playlist activa, último ping formateado. |
| T-02.FE.03 | Implementar `ScreenService.getScreens()` | Llama a `GET /api/screens`. Retorna observable. |
| T-02.FE.04 | Suscribirse a SignalR para updates en tiempo real | Conectar a hub `/signalr/admin`. Escuchar `screenStatusUpdated`. Actualizar estado local sin reload. |
| T-02.FE.05 | Estilo visual: badge online (verde) / offline (rojo) | Conforme paleta Sichi: rojo #E53935 para offline / verde para online. |

---

### Pantallas

---

#### HU-03 — Gestión de pantallas

> Como administrador, quiero crear, editar y eliminar pantallas para mantener el inventario de TVs del local.

**Criterios de aceptación:**
- CRUD completo de pantallas (nombre, screenKey)
- screenKey es único e inmutable una vez creado
- No se puede eliminar una pantalla con playlist asignada

**Tasks — Backend:**

| ID | Task | Detalle |
|---|---|---|
| T-03.BE.01 | Crear entidad `Screen` y migración EF Core | Campos: Id, Name, ScreenKey (UNIQUE), CurrentPlaylistId (nullable FK), IsOnline, LastPing, CreatedAt. |
| T-03.BE.02 | Implementar `GET /api/screens` | Lista todas las pantallas con estado calculado. |
| T-03.BE.03 | Implementar `GET /api/screens/{id}` | Detalle de una pantalla. |
| T-03.BE.04 | Implementar `POST /api/screens` | Recibe `{ name, screenKey }`. Valida unicidad de screenKey. |
| T-03.BE.05 | Implementar `PUT /api/screens/{id}` | Permite editar solo `Name`. `ScreenKey` es inmutable después de creación. |
| T-03.BE.06 | Implementar `DELETE /api/screens/{id}` | Rechaza con HTTP 409 si tiene playlist asignada. |
| T-03.BE.07 | Registrar en AuditLog: crear y eliminar pantalla | Acción: `ScreenCreated`, `ScreenDeleted`. |

**Tasks — Frontend:**

| ID | Task | Detalle |
|---|---|---|
| T-03.FE.01 | Crear vista `ScreensListComponent` | Tabla con nombre, screenKey, estado, playlist asignada, acciones. |
| T-03.FE.02 | Crear modal/form `ScreenFormComponent` | Para crear y editar. Campo screenKey visible pero no editable en modo edición. |
| T-03.FE.03 | Implementar `ScreenService`: `create`, `update`, `delete` | Métodos que llaman a los endpoints correspondientes. |
| T-03.FE.04 | Confirmación antes de eliminar | Dialog de confirmación. Mostrar error si pantalla tiene playlist asignada. |
| T-03.FE.05 | Validación de screenKey en frontend | Solo caracteres alfanuméricos y guiones. Minúsculas. No editable en modo edición. |

---

#### HU-04 — Asignar playlist a pantalla

> Como administrador, quiero asignar una playlist a una pantalla para definir qué contenido se reproduce en cada TV.

**Criterios de aceptación:**
- Selector de playlist disponible en la vista de pantalla
- Cambio persiste en BD y se notifica vía SignalR al player
- Una pantalla puede tener solo una playlist a la vez

**Tasks — Backend:**

| ID | Task | Detalle |
|---|---|---|
| T-04.BE.01 | Implementar `PUT /api/screens/{id}/playlist` | Body: `{ playlistId }`. Actualiza `Screen.CurrentPlaylistId`. Admite null para desasignar. |
| T-04.BE.02 | Emitir evento SignalR `playlistChanged` al player tras asignación | Emitir a grupo `screen_{screenKey}` con `{ playlistId, version }`. |
| T-04.BE.03 | Registrar en AuditLog: asignación de playlist | Acción: `PlaylistAssigned`. Detalle: `screenId`, `playlistId`. |

**Tasks — Frontend:**

| ID | Task | Detalle |
|---|---|---|
| T-04.FE.01 | Agregar selector de playlist en `ScreenDetailComponent` | Dropdown con playlists activas disponibles. Opción "Sin playlist". |
| T-04.FE.02 | Implementar `ScreenService.assignPlaylist(screenId, playlistId)` | Llama a `PUT /api/screens/{id}/playlist`. |
| T-04.FE.03 | Confirmar asignación con feedback visual | Toast/snackbar de confirmación tras asignar exitosamente. |

---

#### HU-05 — Forzar refresh de pantalla

> Como administrador, quiero forzar la recarga del player en una pantalla específica para resolver problemas sin intervención física.

**Criterios de aceptación:**
- Botón "Forzar refresh" en detalle de pantalla
- Emite evento SignalR `forceRefresh` al grupo de esa pantalla
- Confirmación visual en el Admin al enviarse

**Tasks — Backend:**

| ID | Task | Detalle |
|---|---|---|
| T-05.BE.01 | Implementar `POST /api/screens/{id}/force-refresh` | Emite evento SignalR `forceRefresh` a grupo `screen_{screenKey}`. Retorna HTTP 200. |

**Tasks — Frontend:**

| ID | Task | Detalle |
|---|---|---|
| T-05.FE.01 | Agregar botón "Forzar refresh" en `ScreenDetailComponent` | Solo visible si pantalla está online. |
| T-05.FE.02 | Implementar `ScreenService.forceRefresh(screenId)` | Llama a `POST /api/screens/{id}/force-refresh`. |
| T-05.FE.03 | Confirmación visual tras enviar | Snackbar: "Señal de refresh enviada a [nombre pantalla]". |

---

### Playlists

---

#### HU-06 — Gestión de playlists

> Como administrador, quiero crear, editar y eliminar playlists para organizar el contenido que se muestra en las pantallas.

**Criterios de aceptación:**
- CRUD completo de playlists (nombre, estado activo/inactivo)
- No se puede eliminar una playlist asignada a una pantalla activa
- Cada edición incrementa el campo `Version` en +1

**Tasks — Backend:**

| ID | Task | Detalle |
|---|---|---|
| T-06.BE.01 | Crear entidad `Playlist` y migración EF Core | Campos: Id, Name, Version (default 1), IsActive, CreatedAt. |
| T-06.BE.02 | Implementar `GET /api/playlists` | Lista todas las playlists con: Id, Name, Version, IsActive, cantidad de ítems. |
| T-06.BE.03 | Implementar `GET /api/playlists/{id}` | Detalle con lista de MediaItems ordenados por SortOrder. |
| T-06.BE.04 | Implementar `POST /api/playlists` | Recibe `{ name }`. Crea con Version=1, IsActive=true. |
| T-06.BE.05 | Implementar `PUT /api/playlists/{id}` | Actualiza nombre y/o estado activo. Incrementa `Version` en +1 por cada edición. |
| T-06.BE.06 | Implementar `DELETE /api/playlists/{id}` | Rechaza con HTTP 409 si hay alguna `Screen.CurrentPlaylistId` apuntando a esta playlist. |
| T-06.BE.07 | Registrar en AuditLog: crear, editar, eliminar playlist | Acciones: `PlaylistCreated`, `PlaylistUpdated`, `PlaylistDeleted`. |

**Tasks — Frontend:**

| ID | Task | Detalle |
|---|---|---|
| T-06.FE.01 | Crear vista `PlaylistsListComponent` | Tabla: nombre, versión, estado activo/inactivo, cantidad de ítems, acciones. |
| T-06.FE.02 | Crear modal/form `PlaylistFormComponent` | Para crear y editar. Campo nombre. Toggle activo/inactivo. |
| T-06.FE.03 | Implementar `PlaylistService`: `getAll`, `getById`, `create`, `update`, `delete` | Llamadas a endpoints correspondientes. |
| T-06.FE.04 | Confirmación antes de eliminar | Dialog de confirmación. Mostrar error HTTP 409 como mensaje amigable. |

---

#### HU-07 — Ordenar ítems de una playlist

> Como administrador, quiero agregar videos a una playlist y definir su orden con drag & drop para controlar la secuencia de reproducción.

**Criterios de aceptación:**
- Agregar / quitar MediaItems a una playlist
- Reordenar ítems con drag & drop
- El orden se persiste en `PlaylistMedia.SortOrder`

**Tasks — Backend:**

| ID | Task | Detalle |
|---|---|---|
| T-07.BE.01 | Crear entidad `PlaylistMedia` y migración EF Core | PK compuesta (PlaylistId, MediaItemId). Campo SortOrder (INT). |
| T-07.BE.02 | Implementar `POST /api/playlists/{id}/media` | Agrega un MediaItem a la playlist. Body: `{ mediaItemId }`. Asigna SortOrder al final. |
| T-07.BE.03 | Implementar `DELETE /api/playlists/{id}/media/{mediaId}` | Quita MediaItem de la playlist. Reordena SortOrder restante para mantener secuencia. |
| T-07.BE.04 | Implementar `PUT /api/playlists/{id}/reorder` | Recibe array de `[{ mediaItemId, sortOrder }]`. Actualiza SortOrder en bloque. Incrementa `Playlist.Version`. |
| T-07.BE.05 | Emitir evento SignalR `playlistChanged` tras reordenar o modificar ítems | Al grupo de cada pantalla asignada a esta playlist. Payload: `{ playlistId, version }`. |

**Tasks — Frontend:**

| ID | Task | Detalle |
|---|---|---|
| T-07.FE.01 | Crear vista `PlaylistDetailComponent` | Muestra ítems actuales de la playlist en orden SortOrder. |
| T-07.FE.02 | Implementar drag & drop con Angular CDK `DragDropModule` | Lista reordenable. Al soltar, persistir nuevo orden via `PUT /api/playlists/{id}/reorder`. |
| T-07.FE.03 | Implementar selector para agregar video a playlist | Modal con lista de MediaItems disponibles (no agregados aún). Botón "Agregar". |
| T-07.FE.04 | Botón eliminar ítem de playlist | Confirmación simple. Llama a `DELETE /api/playlists/{id}/media/{mediaId}`. |
| T-07.FE.05 | Mostrar metadata del ítem: nombre, duración, miniatura (si aplica) | Dentro de cada fila de la lista drag & drop. |

---

### Media

---

#### HU-08 — Biblioteca de videos

> Como administrador, quiero subir y eliminar videos para gestionar el contenido disponible en el sistema.

**Criterios de aceptación:**
- Subir archivos `.mp4` H264, máximo 50 MB
- Validación de formato y tamaño en backend
- Generación de checksum SHA256 al subir
- Almacenamiento en `/uploads/media/YYYY/MM/`
- Eliminar video: no permitido si está en una playlist activa
- Vista de biblioteca: nombre, duración, tamaño, playlists donde se usa

**Tasks — Backend:**

| ID | Task | Detalle |
|---|---|---|
| T-08.BE.01 | Crear entidad `MediaItem` y migración EF Core | Campos: Id, FileName, FilePath, MediaType (enum 'Video'), DurationSeconds, Checksum (SHA256), FileSize, CreatedAt. |
| T-08.BE.02 | Implementar `POST /api/media/upload` | Recibe `multipart/form-data`. Valida extensión `.mp4` y tamaño ≤ 50MB. |
| T-08.BE.03 | Validación de formato H264 en backend | Verificar cabecera del archivo o usar `MediaInfo` / `ffprobe` para confirmar codec. Rechazar si no es H264. |
| T-08.BE.04 | Generar SHA256 del archivo al subir | Calcular sobre stream del archivo. Almacenar en `MediaItem.Checksum`. |
| T-08.BE.05 | Guardar archivo en disco `/uploads/media/YYYY/MM/` | Crear subdirectorios por año/mes. Filename: UUID + extensión original. |
| T-08.BE.06 | Extraer duración del video | Usar `ffprobe` o parsear metadata MP4. Almacenar en `DurationSeconds`. |
| T-08.BE.07 | Implementar `GET /api/media` | Lista todos los MediaItems: Id, FileName, DurationSeconds, FileSize, checksum, playlists donde aparece. |
| T-08.BE.08 | Implementar `DELETE /api/media/{id}` | Rechaza HTTP 409 si el ítem está en alguna playlist. Elimina registro BD y archivo físico. |
| T-08.BE.09 | Registrar en AuditLog: subir y eliminar video | Acciones: `MediaUploaded`, `MediaDeleted`. |

**Tasks — Frontend:**

| ID | Task | Detalle |
|---|---|---|
| T-08.FE.01 | Crear vista `MediaLibraryComponent` | Tabla/grid: nombre, duración (mm:ss), tamaño (MB), playlists donde se usa, acciones. |
| T-08.FE.02 | Implementar upload con progreso visual | Input `type="file"` filtrado a `.mp4`. Barra de progreso usando `HttpClient` con `reportProgress`. |
| T-08.FE.03 | Implementar `MediaService`: `getAll`, `upload`, `delete` | Llamadas a endpoints correspondientes. |
| T-08.FE.04 | Validación frontend antes de subir | Verificar extensión `.mp4` y tamaño ≤ 50MB antes del request. Mensaje de error claro. |
| T-08.FE.05 | Confirmación antes de eliminar | Dialog de confirmación. Mostrar error HTTP 409 como mensaje amigable si está en playlist activa. |

---

### Player TV

---

#### HU-09 — Reproducción continua en pantalla

> Como pantalla de TV, quiero reproducir la playlist asignada en bucle continuo para mostrar contenido sin interrupciones.

**Criterios de aceptación:**
- Identifica la pantalla por `?screen=screenKey` en la URL
- Obtiene playlist activa desde `GET /api/player/playlist`
- Descarga y cachea assets en IndexedDB
- Verifica checksum SHA256 antes de reproducir
- Carrusel en bucle usando `<video>` nativo
- Si no hay playlist asignada: muestra pantalla de espera con logo Sichi

**Tasks — Player (HTML/Vanilla JS):**

| ID | Task | Detalle |
|---|---|---|
| T-09.PL.01 | Parsear `?screen=screenKey` de la URL al iniciar | `URLSearchParams`. Si no existe el param, mostrar error "Pantalla no configurada". |
| T-09.PL.02 | Llamar a `GET /api/player/playlist?screen=xxx` al iniciar | Obtener estructura de playlist: id, version, array de items `[{ id, filePath, checksum, durationSeconds }]`. |
| T-09.PL.03 | Implementar descarga y cacheo de assets en IndexedDB | Para cada item: si ya existe en caché con mismo checksum, no re-descargar. Si no, descargar como Blob y almacenar. |
| T-09.PL.04 | Verificar SHA256 de cada asset descargado | Calcular SHA256 del Blob recibido usando WebCrypto API. Rechazar y re-descargar si no coincide. |
| T-09.PL.05 | Implementar carrusel en bucle con `<video>` nativo | Al terminar cada video (`ended` event), reproducir el siguiente. Al llegar al último, volver al primero. |
| T-09.PL.06 | Pantalla de espera si no hay playlist asignada | Mostrar logo Sichi centrado sobre fondo negro. Reintentar `GET /api/player/playlist` cada 60 segundos. |
| T-09.PL.07 | Autoplay en modo kiosco: atributos `autoplay muted playsinline` | Requerido para autoplay sin interacción de usuario en browsers modernos. |

**Tasks — Backend:**

| ID | Task | Detalle |
|---|---|---|
| T-09.BE.01 | Implementar `GET /api/player/playlist?screen=xxx` | Busca Screen por ScreenKey. Retorna playlist con items ordenados por SortOrder. Sin autenticación JWT. |
| T-09.BE.02 | Servir archivos de media estáticos | Configurar Nginx o .NET Static Files para `/uploads/media/`. Accesible por URL pública. |

---

#### HU-10 — Sincronización automática de contenido

> Como pantalla de TV, quiero recibir actualizaciones de contenido automáticamente para reflejar cambios sin intervención manual.

**Criterios de aceptación:**
- Conecta a SignalR al cargar (`/signalr/player`, grupo `screen_{screenKey}`)
- Al recibir `playlistChanged`: descarga nuevos assets, verifica checksum, actualiza caché
- Fallback: polling cada 5 minutos a `GET /api/player/version`
- Nunca interrumpe la reproducción actual hasta tener los nuevos assets listos

**Tasks — Backend:**

| ID | Task | Detalle |
|---|---|---|
| T-10.BE.01 | Implementar `PlayerHub` en SignalR | Hub en `/signalr/player`. Método `screenConnected(screenKey)` que agrega al grupo `screen_{screenKey}`. |
| T-10.BE.02 | Implementar `GET /api/player/version?screen=xxx` | Retorna `{ playlistId, version }` actual para la pantalla. Para fallback polling. |

**Tasks — Player (HTML/Vanilla JS):**

| ID | Task | Detalle |
|---|---|---|
| T-10.PL.01 | Conectar a SignalR al iniciar | Usar `@microsoft/signalr` CDN. Conectar a `/signalr/player`. Llamar `screenConnected(screenKey)`. |
| T-10.PL.02 | Escuchar evento `playlistChanged` | Al recibirlo: descargar nuevos assets en segundo plano sin interrumpir reproducción actual. |
| T-10.PL.03 | Swap de playlist sin interrumpir reproducción | Construir nueva cola de reproducción con assets nuevos. Aplicar en el siguiente ciclo del carrusel. |
| T-10.PL.04 | Implementar fallback polling cada 5 minutos | Si SignalR desconectado: `setInterval` a `GET /api/player/version`. Si version cambió, descargar nuevos assets. |
| T-10.PL.05 | Manejo de reconexión SignalR | Usar `withAutomaticReconnect()`. Al reconectar, verificar version inmediatamente. |

---

#### HU-11 — Operación offline garantizada

> Como pantalla de TV, quiero seguir reproduciendo contenido aunque se pierda la conexión a internet para evitar pantallas negras.

**Criterios de aceptación:**
- Si no hay conexión al iniciar: reproduce última playlist en caché (IndexedDB)
- Si SignalR se desconecta: continúa reproducción y activa polling
- Pantalla negra: nunca permitida

**Tasks — Player (HTML/Vanilla JS):**

| ID | Task | Detalle |
|---|---|---|
| T-11.PL.01 | Al iniciar: intentar fetch de playlist, si falla cargar desde IndexedDB | `try/catch` en el fetch inicial. Si catch: leer playlist cacheada de IndexedDB y reproducir. |
| T-11.PL.02 | Estructura de IndexedDB para playlist y assets | Object stores: `playlists` (id, version, items[]), `assets` (id, blob, checksum). |
| T-11.PL.03 | Guardar siempre la playlist activa en IndexedDB tras sync exitoso | Después de cada descarga y verificación exitosa, persistir playlist y assets. |
| T-11.PL.04 | Si video falla al reproducirse: saltar al siguiente sin pantalla negra | Escuchar evento `error` en `<video>`. Avanzar al siguiente ítem de la cola inmediatamente. |
| T-11.PL.05 | Listener de `online`/`offline` del browser | Al recuperar conexión (`online` event): reintentar fetch de versión y sincronizar si hay cambios. |

---

### Heartbeat & Audit

---

#### HU-12 — Heartbeat de pantallas

> Como sistema, quiero registrar pings periódicos de cada pantalla para conocer su estado de conectividad en tiempo real.

**Criterios de aceptación:**
- Player envía `POST /api/player/heartbeat` cada 30 segundos
- Backend actualiza `Screen.LastPing` e `IsOnline`
- Pantalla marcada offline si no hay ping en más de 60 segundos

**Tasks — Backend:**

| ID | Task | Detalle |
|---|---|---|
| T-12.BE.01 | Implementar `POST /api/player/heartbeat` | Body: `{ screenKey, timestamp }`. Busca Screen por ScreenKey. Actualiza LastPing y IsOnline=true. |
| T-12.BE.02 | Job periódico para marcar pantallas offline | `IHostedService` o `BackgroundService` que cada 30 segundos marca como offline las pantallas sin ping en los últimos 60 segundos. |
| T-12.BE.03 | Emitir evento SignalR `screenStatusUpdated` tras heartbeat | Emitir al grupo `admin` con el estado actualizado de la pantalla para refrescar el Dashboard. |

**Tasks — Player (HTML/Vanilla JS):**

| ID | Task | Detalle |
|---|---|---|
| T-12.PL.01 | Enviar heartbeat cada 30 segundos | `setInterval` con `fetch POST /api/player/heartbeat`. Incluir screenKey. |
| T-12.PL.02 | Manejar fallo del heartbeat sin interrumpir reproducción | `try/catch` silencioso. Log en consola. No afectar la reproducción. |

---

#### HU-13 — Historial de cambios

> Como administrador, quiero ver un registro de las acciones realizadas en el sistema para auditar cambios en contenido y pantallas.

**Criterios de aceptación:**
- AuditLog registra: acción, entidad afectada, detalle, fecha
- Eventos auditados: login, crear/editar/eliminar playlist, subir/eliminar video, asignar playlist
- Vista de historial en el Admin: tabla ordenada por fecha descendente

**Tasks — Backend:**

| ID | Task | Detalle |
|---|---|---|
| T-13.BE.01 | Crear entidad `AuditLog` y migración EF Core | Campos: Id, Action, UserId (FK), EntityId, Detail (TEXT), CreatedAt. |
| T-13.BE.02 | Implementar `IAuditService` | Método `LogAsync(action, userId, entityId, detail)`. Inyectado en use cases que lo requieren. |
| T-13.BE.03 | Implementar `GET /api/audit` | Retorna lista paginada ordenada por CreatedAt DESC. Query params: `page`, `pageSize` (default 50). |
| T-13.BE.04 | Registrar evento de login exitoso | En `POST /api/auth/login` exitoso: log `UserLoggedIn`. |

**Tasks — Frontend:**

| ID | Task | Detalle |
|---|---|---|
| T-13.FE.01 | Crear vista `AuditLogComponent` | Tabla con columnas: fecha, acción, detalle. Ordenada por fecha descendente. |
| T-13.FE.02 | Implementar `AuditService.getAll()` | Llama a `GET /api/audit`. |
| T-13.FE.03 | Paginación básica en la vista | "Cargar más" o paginación simple. Evitar cargar todo el historial en una sola llamada. |

---

### Infraestructura

---

#### TASK-INF — Infraestructura Base (Día 1)

> Tareas de infraestructura no asociadas a HU funcional, necesarias para iniciar el desarrollo.

**Tasks:**

| ID | Task | Detalle |
|---|---|---|
| T-INF.01 | Crear `docker-compose.yml` | Servicios: `api` (.NET 8), `db` (MySQL 8), `nginx`. Redes internas. Volúmenes para BD y uploads. |
| T-INF.02 | Crear `Dockerfile` para la API .NET 8 | Multi-stage build: `sdk` para build, `aspnet` runtime para producción. |
| T-INF.03 | Crear estructura de proyecto .NET con Clean Architecture | Directorios: `Domain/`, `Application/`, `Infrastructure/`, `API/`. Proyectos separados por capa. |
| T-INF.04 | Configurar EF Core con MySQL 8 | Instalar `Pomelo.EntityFrameworkCore.MySql`. Configurar `DbContext`. Connection string desde env var. |
| T-INF.05 | Crear proyecto Angular con estructura base | `ng new sichi-admin`. Módulos: `Auth`, `Screens`, `Playlists`, `Media`, `Dashboard`, `Audit`. |
| T-INF.06 | Configurar `nginx.conf` | Reverse proxy: `/api/` → API, `/signalr/` → API (WebSocket upgrade), `/` → Angular SPA, `/uploads/` → static files. |
| T-INF.07 | Configurar variables de entorno para producción | `.env` con: `DB_CONNECTION`, `JWT_SECRET`, `ADMIN_EMAIL`, `ADMIN_PASSWORD`. Nunca en repositorio. |
| T-INF.08 | Configurar Let's Encrypt + Certbot en VPS | Script de instalación automática. Renovación automática via cron. |
| T-INF.09 | Configurar `Health endpoint` `GET /api/health` | Retorna HTTP 200 con `{ status: "healthy", timestamp }`. Para monitoreo y smoke tests. |

---

## 4. Sprint Único — 5 Días

| Día | Bloque | HUs / Tareas |
|---|---|---|
| 1 | Infraestructura base | T-INF.01–09: Docker + Compose, BD MySQL, migraciones EF Core, proyecto .NET scaffolding, proyecto Angular scaffolding |
| 2 | Backend core | HU-01 (Auth), HU-03 (Pantallas), HU-06 (Playlists), HU-08 (Media) |
| 3 | Backend tiempo real + Player | HU-12 (Heartbeat), HU-09 + HU-10 + HU-11 (Player TV), SignalR Hub |
| 4 | Frontend Admin | HU-02 (Dashboard), HU-04 (Asignar playlist), HU-05 (Force refresh), HU-07 (Drag & drop), HU-13 (Historial) |
| 5 | Integración, deploy y smoke test | Nginx + SSL, VPS deploy, prueba end-to-end con Firestick real (R-01) |

---

## 5. Conteo de Tasks por HU

| HU | Módulo | Tasks BE | Tasks FE/PL | Tasks Infra | Total |
|---|---|---|---|---|---|
| HU-01 | Auth | 6 | 6 | — | 12 |
| HU-02 | Dashboard | 3 | 5 | — | 8 |
| HU-03 | Pantallas — CRUD | 7 | 5 | — | 12 |
| HU-04 | Pantallas — Asignar playlist | 3 | 3 | — | 6 |
| HU-05 | Pantallas — Force refresh | 1 | 3 | — | 4 |
| HU-06 | Playlists — CRUD | 7 | 4 | — | 11 |
| HU-07 | Playlists — Drag & drop | 5 | 5 | — | 10 |
| HU-08 | Media | 9 | 5 | — | 14 |
| HU-09 | Player — Reproducción | 7 (PL) + 2 (BE) | — | — | 9 |
| HU-10 | Player — Sync | 2 (BE) + 5 (PL) | — | — | 7 |
| HU-11 | Player — Offline | 5 (PL) | — | — | 5 |
| HU-12 | Heartbeat | 3 (BE) + 2 (PL) | — | — | 5 |
| HU-13 | Audit | 4 | 3 | — | 7 |
| INF | Infraestructura | — | — | 9 | 9 |
| **Total** | | | | | **119** |

---

## 6. Dependencias Externas

| Dependencia | Tipo | Impacto |
|---|---|---|
| VPS Linux (Hostinger / Hetzner) | Infraestructura | Necesario desde Día 5 |
| Dominio y DNS configurado | Infraestructura | Necesario desde Día 5 |
| Firestick físico disponible | Hardware | Spike técnico en Día 5 (R-01) |
| Videos `.mp4 H264` de prueba | Contenido | Necesario desde Día 3 |
| `ffprobe` / `ffmpeg` en VPS | Herramienta | Para T-08.BE.03 (validación H264) y T-08.BE.06 (duración) |
| `@microsoft/signalr` CDN | Librería | Para Player TV (T-10.PL.01) |
| Angular CDK `DragDropModule` | Librería | Para T-07.FE.02 |

---

## 7. Riesgos Vigentes

| ID | Riesgo | Probabilidad | Impacto | Mitigación | Task relacionado |
|---|---|---|---|---|---|
| R-01 | Browser del Firestick (Silk/Firefox) con limitaciones en IndexedDB o video playback | Media | Alto | Spike técnico en Día 5 con hardware real | T-09.PL.07, T-11.PL.01 |
| R-02 | Assets de video (hasta 50MB × N pantallas) saturan el VPS en picos de sync simultáneo | Baja-Media | Medio | Evaluar sync escalonado en Node 3 | T-10.PL.02 |
| R-03 | Scope creep hacia analytics de reproducciones | Alta | Medio | Documentado explícitamente fuera de scope Fase 1 | — |
| R-04 | `ffprobe` / validación H264 agrega complejidad en Día 2 | Media | Bajo | Como fallback: validar solo extensión y tamaño; H264 se verifica en Node 3 | T-08.BE.03, T-08.BE.06 |

---

## 8. Gate 2 — APROBADO

| Criterio | Estado |
|---|---|
| Sprint definido | ✅ |
| Epic e HUs escritas | ✅ |
| Tasks detallados por HU | ✅ |
| Orden de desarrollo establecido | ✅ |
| Dependencias externas identificadas | ✅ |
| Riesgos de Node 1 incorporados | ✅ |

**Node 2 CLOSED. Listo para Node 3 — Technical Architecture & Engineering Strategy.**

---

*Documento generado el 2026-05-31*
*Node 2 — Delivery Planning — Sichi Digital Signage*
