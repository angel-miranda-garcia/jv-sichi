# delivery_plan.md
## Sichi Digital Signage — Node 2 Output
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

## 3. Historias de Usuario

### Auth

**HU-01 — Login de administrador**
> Como administrador, quiero iniciar sesión con usuario y contraseña para acceder al panel de gestión.

- Formulario de login con usuario y contraseña
- JWT devuelto en respuesta, almacenado en cliente
- Contraseña hasheada con bcrypt en la BD
- Redirige al dashboard tras login exitoso
- Muestra error genérico en credenciales inválidas

---

### Dashboard

**HU-02 — Estado en vivo de pantallas**
> Como administrador, quiero ver en el dashboard el estado actual de cada pantalla para saber cuáles están activas y qué contenido están reproduciendo.

- Lista de pantallas con: nombre, estado online/offline, playlist activa, último ping
- Estado calculado: online si último ping < 60 segundos
- Actualización en tiempo real vía SignalR
- Sin acciones desde el dashboard (solo lectura)

---

### Pantallas

**HU-03 — Gestión de pantallas**
> Como administrador, quiero crear, editar y eliminar pantallas para mantener el inventario de TVs del local.

- CRUD completo de pantallas (nombre, screenKey)
- screenKey es único e inmutable una vez creado
- No se puede eliminar una pantalla con playlist asignada

**HU-04 — Asignar playlist a pantalla**
> Como administrador, quiero asignar una playlist a una pantalla para definir qué contenido se reproduce en cada TV.

- Selector de playlist disponible en la vista de pantalla
- Cambio persiste en BD y se notifica vía SignalR al player
- Una pantalla puede tener solo una playlist a la vez

**HU-05 — Forzar refresh de pantalla**
> Como administrador, quiero forzar la recarga del player en una pantalla específica para resolver problemas sin intervención física.

- Botón "Forzar refresh" en detalle de pantalla
- Emite evento SignalR `forceRefresh` al grupo de esa pantalla
- Confirmación visual en el Admin al enviarse

---

### Playlists

**HU-06 — Gestión de playlists**
> Como administrador, quiero crear, editar y eliminar playlists para organizar el contenido que se muestra en las pantallas.

- CRUD completo de playlists (nombre, estado activo/inactivo)
- No se puede eliminar una playlist asignada a una pantalla activa
- Cada edición incrementa el campo `Version` en +1

**HU-07 — Ordenar ítems de una playlist**
> Como administrador, quiero agregar videos a una playlist y definir su orden con drag & drop para controlar la secuencia de reproducción.

- Agregar / quitar MediaItems a una playlist
- Reordenar ítems con drag & drop
- El orden se persiste en `PlaylistMedia.SortOrder`

---

### Media

**HU-08 — Biblioteca de videos**
> Como administrador, quiero subir y eliminar videos para gestionar el contenido disponible en el sistema.

- Subir archivos `.mp4` H264, máximo 50 MB
- Validación de formato y tamaño en backend
- Generación de checksum SHA256 al subir
- Almacenamiento en `/uploads/media/YYYY/MM/`
- Eliminar video: no permitido si está en una playlist activa
- Vista de biblioteca: nombre, duración, tamaño, playlists donde se usa

---

### Player TV

**HU-09 — Reproducción continua en pantalla**
> Como pantalla de TV, quiero reproducir la playlist asignada en bucle continuo para mostrar contenido sin interrupciones.

- Identifica la pantalla por `?screen=screenKey` en la URL
- Obtiene playlist activa desde `GET /api/player/playlist`
- Descarga y cachea assets en IndexedDB
- Verifica checksum SHA256 antes de reproducir
- Carrusel en bucle usando `<video>` nativo
- Si no hay playlist asignada: muestra pantalla de espera con logo Sichi

**HU-10 — Sincronización automática de contenido**
> Como pantalla de TV, quiero recibir actualizaciones de contenido automáticamente para reflejar cambios sin intervención manual.

- Conecta a SignalR al cargar (`/signalr/player`, grupo `screen_{screenKey}`)
- Al recibir `playlistChanged`: descarga nuevos assets, verifica checksum, actualiza caché
- Fallback: polling cada 5 minutos a `GET /api/player/version`
- Nunca interrumpe la reproducción actual hasta tener los nuevos assets listos

**HU-11 — Operación offline garantizada**
> Como pantalla de TV, quiero seguir reproduciendo contenido aunque se pierda la conexión a internet para evitar pantallas negras.

- Si no hay conexión al iniciar: reproduce última playlist en caché (IndexedDB)
- Si SignalR se desconecta: continúa reproducción y activa polling
- Pantalla negra: nunca permitida

---

### Heartbeat & Audit

**HU-12 — Heartbeat de pantallas**
> Como sistema, quiero registrar pings periódicos de cada pantalla para conocer su estado de conectividad en tiempo real.

- Player envía `POST /api/player/heartbeat` cada 30 segundos
- Backend actualiza `Screen.LastPing` e `IsOnline`
- Pantalla marcada offline si no hay ping en más de 60 segundos

**HU-13 — Historial de cambios**
> Como administrador, quiero ver un registro de las acciones realizadas en el sistema para auditar cambios en contenido y pantallas.

- AuditLog registra: acción, entidad afectada, detalle, fecha
- Eventos auditados: login, crear/editar/eliminar playlist, subir/eliminar video, asignar playlist
- Vista de historial en el Admin: tabla ordenada por fecha descendente

---

## 4. Sprint Único — 5 Días

> El tiempo es referencial. El sprint agrupa el trabajo en bloques lógicos de desarrollo, no en días calendario estrictos.

| Día | Bloque | HUs / Tareas |
|---|---|---|
| 1 | Infraestructura base | Docker + Compose, BD MySQL, migraciones EF Core, proyecto .NET scaffolding, proyecto Angular scaffolding |
| 2 | Backend core | HU-01 (Auth), HU-03 (Pantallas), HU-06 (Playlists), HU-08 (Media) |
| 3 | Backend tiempo real + Player | HU-12 (Heartbeat), HU-09 + HU-10 + HU-11 (Player TV), SignalR Hub |
| 4 | Frontend Admin | HU-02 (Dashboard), HU-04 (Asignar playlist), HU-05 (Force refresh), HU-07 (Drag & drop), HU-13 (Historial) |
| 5 | Integración, deploy y smoke test | Nginx + SSL, VPS deploy, prueba end-to-end con Firestick real (R-01) |

---

## 5. Dependencias Externas

| Dependencia | Tipo | Impacto |
|---|---|---|
| VPS Linux (Hostinger / Hetzner) | Infraestructura | Necesario desde Día 5 |
| Dominio y DNS configurado | Infraestructura | Necesario desde Día 5 |
| Firestick físico disponible | Hardware | Spike técnico en Día 5 (R-01) |
| Videos `.mp4 H264` de prueba | Contenido | Necesario desde Día 3 |

---

## 6. Gate 2 — APROBADO

| Criterio | Estado |
|---|---|
| Sprint definido | ✅ |
| Epic e HUs escritas | ✅ |
| Orden de desarrollo establecido | ✅ |
| Dependencias externas identificadas | ✅ |
| Riesgos de Node 1 incorporados | ✅ |

**Node 2 CLOSED. Listo para Node 3 — Technical Architecture & Engineering Strategy.**

---

*Documento generado el 2026-05-31*
*Node 2 — Delivery Planning — Sichi Digital Signage*
