# Especificación de Negocio — Sichi Digital Signage

**Cliente:** Jesús V. Cardiel  
**Proyecto:** Plataforma de Señalización Digital — Restaurante Sichi (Sushi)  
**Fecha:** 31 de mayo de 2026  
**Arquitecto:** Ángel M. García

---

## 1. Contexto del Negocio

| Campo | Valor |
|---|---|
| Cliente | Sichi — Restaurante de sushi |
| Pantallas por sucursal | 3 a 5 |
| Frecuencia de cambio de contenido | 1 vez al día (o por horario/fecha configurable) |
| Usuarios del Admin | 2 a 3 (Gerente + Encargado de sucursal) |

---

## 2. Roles y Permisos

### Gerente — Acceso total

- Gestionar usuarios (crear, desactivar, cambiar contraseñas)
- Crear y editar playlists
- Subir y eliminar media
- Asignar playlists a pantallas
- Ver estado en vivo de pantallas
- Programar horarios y fechas de contenido
- Ver historial completo de cambios

### Encargado de sucursal — Acceso operativo

- Crear y editar playlists
- Subir media
- Asignar playlists a pantallas
- Ver estado en vivo de pantallas
- ✕ Sin gestión de usuarios
- ✕ Sin acceso al historial completo

---

## 3. Tipos de Contenido

| Tipo | Descripción | Formato |
|---|---|---|
| Menú del día | Platillos activos | Imágenes .webp |
| Promociones | Combos y ofertas | Imágenes o videos cortos |
| Precios | Lista de precios | Imágenes .webp |
| Institucional | Ambiente / marca | Videos .mp4 |

---

## 4. Reglas de Negocio

1. Cada pantalla puede tener una playlist diferente o compartir la misma con otras.
2. Una playlist contiene N ítems de media ordenados manualmente.
3. Cada ítem tiene duración configurable en segundos.
4. El contenido puede programarse por **horario** (ej. menú comida 12pm–3pm) o por **fecha** (ej. promo fin de semana).
5. Si no hay programación activa, se reproduce la playlist base asignada a la pantalla.
6. Al actualizar una playlist, las pantallas se sincronizan automáticamente vía SignalR.
7. Si la pantalla pierde internet, sigue reproduciendo la última playlist descargada en caché local (IndexedDB).
8. Una pantalla sin playlist asignada muestra una pantalla de espera con logo de Sichi.
9. El gerente puede forzar una actualización inmediata a cualquier pantalla desde el Admin.
10. Los medios subidos se validan:
    - Imágenes: máx 2MB (.webp / .jpg / .png → convertir automáticamente a .webp)
    - Videos: máx 50MB (.mp4 H264 únicamente)
11. Una imagen o video no puede eliminarse si está en uso en alguna playlist activa.

---

## 5. Programación de Contenido (Schedule)

### Tipos de schedule

| Tipo | Ejemplo |
|---|---|
| Por horario | Lunes–Viernes 12pm–3pm → Playlist "Menú Comida" |
| Por fecha | Sáb 31 May – Dom 1 Jun → Playlist "Promo Fin de Semana" |

### Regla de prioridad (cuando hay conflicto)

```
Fecha específica  >  Horario recurrente  >  Playlist base
```

La programación más específica siempre gana.

### Comportamiento sin schedule activo

La pantalla reproduce la playlist base que el gerente le asignó por defecto.

---

## 6. Módulos del Admin (Pantallas UI)

| Módulo | Rol | Descripción |
|---|---|---|
| Dashboard | Todos | Estado en vivo: online/offline, playlist activa, último ping de cada pantalla |
| Pantallas | Todos | Listado de TVs, asignar playlist, ver estado, forzar refresh |
| Playlists | Todos | Crear, editar, ordenar ítems con drag & drop, previsualizar |
| Media | Todos | Biblioteca de imágenes y videos, subir, eliminar, ver en qué playlists se usa |
| Programación | Todos | Crear schedules por horario o fecha, asignar a pantallas |
| Usuarios | Solo gerente | Crear y desactivar encargados, cambiar contraseñas |
| Historial | Solo gerente | Log de cambios: quién cambió qué y cuándo |

---

## 7. Flujo Principal — Actualizar Contenido

```
1. Subir media
   Gerente o encargado sube imagen/video desde el Admin.
   El backend valida, comprime y guarda en /uploads.

2. Editar playlist
   Agrega el nuevo ítem, define duración y orden.
   La versión de la playlist sube +1.

3. Notificación push
   La API emite evento SignalR "playlistChanged"
   a las pantallas asignadas a esa playlist.

4. Sync en TV
   El player descarga los nuevos assets,
   verifica checksum y guarda en IndexedDB.

5. Renderizado
   El carrusel se actualiza en el siguiente ciclo.
   Sin cortes visuales, sin pantalla negra.
```

---

## 8. Flujo de Arranque del Player (TV)

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
   │     NO → Renderizar desde caché local
   │
   └─→ Loop de reproducción continua
```

> **Regla de oro:** Nunca renderizar antes de tener los assets descargados y verificados.

---

## 9. Comportamiento Offline

| Situación | Comportamiento esperado |
|---|---|
| Sin internet | Reproduce última playlist descargada en caché |
| SignalR caído | Polling cada 5 min a GET /api/player/version |
| Corte de luz | Android TV Box arranca solo, abre player automáticamente en kiosko |
| Sin playlist asignada | Muestra pantalla de espera con logo de Sichi |
| Pantalla negra | Nunca permitida bajo ninguna circunstancia |

---

## 10. Requerimientos No Funcionales

| Requerimiento | Detalle |
|---|---|
| Disponibilidad | La TV nunca muestra pantalla negra |
| Auto-recovery | El Android TV Box arranca solo tras corte de luz |
| Rendimiento Admin | Respuesta menor a 2 segundos en operaciones normales |
| Heartbeat | El player hace ping cada 30 segundos |
| Fallback SignalR | Polling cada 5 minutos si WebSocket cae |
| Descarga segura | Assets descargados y verificados antes de renderizar |
| Escalabilidad | 1 a 20 pantallas por sucursal sin cambios de arquitectura |
| Escalabilidad futura | 1 a 10 sucursales sin reescribir el sistema |

---

## 11. Estilo Visual del Admin

| Aspecto | Definición |
|---|---|
| Paleta | Rojo Sichi (#E53935) + negro + blanco + gris claro |
| Tono | Vivo y llamativo, acentos en rojo |
| Referencia visual | Japonés moderno — clean, tipografía fuerte, íconos claros |
| Responsive | Desktop first (el Admin se usa en PC o tablet, no en móvil) |

---

## 12. Restricciones Técnicas Derivadas del Negocio

- Imágenes: solo .webp en almacenamiento (conversión automática al subir)
- Videos: solo .mp4 H264 (evitar .mov, .avi, .mkv — compatibilidad limitada en Android TV)
- Tamaño máximo imagen: 2MB
- Tamaño máximo video: 50MB
- Un media en uso no puede eliminarse
- La versión de playlist es obligatoria para detectar cambios sin recargar todo

---

## 13. Modelo de Datos — Tabla Schedule (adicional)

Esta tabla es necesaria para soportar la programación de contenido:

```
Schedule
─────────────────────────────────────
Id               INT PK AUTO_INCREMENT
ScreenId         INT FK → Screen
PlaylistId       INT FK → Playlist
Type             ENUM('Horario', 'Fecha')
-- Si Type = Horario:
DaysOfWeek       VARCHAR(20)   -- "1,2,3,4,5" (lunes a viernes)
StartTime        TIME          -- 12:00:00
EndTime          TIME          -- 15:00:00
-- Si Type = Fecha:
StartDate        DATE
EndDate          DATE
IsActive         BOOLEAN
CreatedAt        DATETIME
```

### Prioridad al resolver qué playlist mostrar

```
1. Buscar Schedule activo de tipo Fecha que cubra hoy
2. Si no → Buscar Schedule activo de tipo Horario que cubra ahora
3. Si no → Usar CurrentPlaylistId de la pantalla (playlist base)
4. Si no hay ninguna → Mostrar pantalla de espera con logo
```

---

*Documento generado el 31 de mayo de 2026*
