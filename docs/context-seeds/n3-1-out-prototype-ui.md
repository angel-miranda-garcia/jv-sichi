# Sichi Digital Signage — Admin Web
## UI/UX Seed · Nodo 4

> Versión: 1.0 · Estado: Aprobado por cliente (VoBo prototipo)
> Stack prototipo: HTML + CSS + JS vanilla, single file
> Credenciales mock: `admin` / `admin`

---

## 1. Identidad Visual

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
| Tono | Japonés moderno — clean, tipografía fuerte, íconos claros |

---

## 2. Layout Global

- **Desktop first**, sin breakpoints responsive requeridos en esta versión
- Sidebar izquierdo fijo + área de contenido scrolleable
- Sidebar colapsable (260px expandido → 80px colapsado)
- Colapsado: oculta textos, centra íconos, borde activo cambia de izquierda a inferior
- Header de página: título dinámico + chip "Admin activo"

---

## 3. Modelo de Datos Mock

```
Pantallas activas:    4  (3 online, 1 offline)
Pantallas pendientes: 1  (auto-registrada, sin aprobar)
Playlists:            3
Media items:          6  (4 video, 2 imagen)
Historial:           45  registros, paginado 10/página → 5 páginas
```

### Mock Pantallas Activas
| Nombre | MAC | Estado | Playlist | Último Ping |
|--------|-----|--------|----------|-------------|
| Menú Principal 1 | 00:1A:2B:3C:4D:5E | Online | Promociones Mayo | Hace 12 seg |
| Barra Central | 00:1A:2B:3C:4D:6F | Online | Clásicos Sichi | Hace 45 seg |
| Terraza Exterior | 00:1A:2B:3C:4D:7A | Offline | Especiales Fin de Semana | Hace 2 horas |
| Entrada Principal | 00:1A:2B:3C:4D:8B | Online | Promociones Mayo | Hace 5 seg |

### Mock Pantallas Pendientes
| Device ID | MAC | Primera Conexión |
|-----------|-----|-----------------|
| DEVICE-A3F2 | 00:1A:2B:3C:4D:9C | Hoy, 14:22 |

### Mock Playlists
| Nombre | Ítems | Duración | Última Modificación |
|--------|-------|----------|---------------------|
| Promociones Mayo | 5 | 00:01:15 | Hoy, 10:30 |
| Clásicos Sichi | 3 | 00:00:45 | Ayer, 18:00 |
| Especiales Fin de Semana | 7 | 00:02:10 | 28 May 2026 |

### Mock Media
| Nombre | Tipo | Peso | Duración | En playlists |
|--------|------|------|----------|--------------|
| promo_2x1_sushi.mp4 | Video | 15 MB | 00:15 | 2 |
| banner_sake.mp4 | Video | 22 MB | 00:20 | 1 |
| ramen_temporada.mp4 | Video | 18 MB | 00:18 | 1 |
| loop_ambient.mp4 | Video | 45 MB | 00:30 | 1 |
| menu_bebidas.jpg | Imagen | 2 MB | — | 0 |
| promo_happy_hour.jpg | Imagen | 1.5 MB | — | 1 |

---

## 4. Navegación y Estructura

### Sidebar — ítems en orden
1. Dashboard
2. Pantallas ← **badge rojo con número** si hay dispositivos en cola de aprobación
3. Playlists
4. Media
5. Historial

### Flujo de navegación
```
Login
  └── App
        ├── Dashboard
        ├── Pantallas
        │     ├── Tab: Activas
        │     └── Tab: Pendientes de aprobación
        ├── Playlists
        │     ├── Vista: Listado
        │     └── Vista: Editar / Crear
        ├── Media
        └── Historial
```

---

## 5. Pantallas por Módulo

---

### 5.1 Login

- Fondo: `--sichi-black`
- Card centrada (max-width 380px), fondo blanco
- Logo "SICHI" en rojo, subtítulo "DIGITAL SIGNAGE"
- Campos: Usuario, Contraseña
- CTA: "Ingresar" (ancho completo, rojo)
- **Error state:** mensaje inline en bloque rojo claro `#FFEBEE`, texto `"Credenciales inválidas. Usa admin / admin."`
- Sin link "olvidé contraseña"

---

### 5.2 Dashboard

**Métricas (grid 3 columnas):**
- Total Pantallas → `4` (negro)
- Online → `3` (verde `--status-online`)
- Offline → `1` (rojo `--status-offline`)

**Tabla "Monitoreo en vivo":**
- Columnas: Ubicación · Estado · Playlist Activa · Último Ping · Acción Rápida
- 4 filas mock
- Badge de estado: pill verde (Online) / pill rojo (Offline) con dot de color
- Botón "Forzar Refresh": activo si Online → toast éxito · **deshabilitado (opacity 0.5, cursor not-allowed) si Offline**

---

### 5.3 Pantallas

**Tab: Activas**
- Columnas: Pantalla / Ubicación · Estado · Playlist Asignada · Último Ping · Acciones
- Sub-texto por nombre: MAC address en `--text-muted`
- Acciones por fila:
  - **Asignar Playlist** → abre `modal-playlist`
  - **Forzar Refresh** → toast éxito (activo siempre, sin validar estado online)
  - **Eliminar** → abre `modal-delete-screen` (confirm simple, sin advertencia de uso)

**Tab: Pendientes de aprobación**
- Columnas: Device ID · MAC · Primera Conexión · Acciones
- Device ID generado automáticamente (formato `DEVICE-XXXX`)
- Acciones por fila:
  - **Aprobar** (btn primary) → fila desaparece, pasa a Activas, toast "Pantalla DEVICE-A3F2 aprobada"
  - **Rechazar** (btn danger outline) → fila desaparece, toast "Dispositivo rechazado"
- **Empty state:** icono monitor + texto "No hay dispositivos pendientes de aprobación"

---

### 5.4 Playlists

**Vista Listado:**
- Columnas: Nombre · Ítems · Duración Total · Modificación · Acciones
- Acciones: Editar · Eliminar
- Eliminar → `modal-delete-playlist` (confirm simple)
- Botón `+ Crear Playlist` (header, btn primary) → abre Vista Editar con nombre vacío

**Vista Editar / Crear:**
- Header sticky: input nombre inline (border-bottom al focus) + botones Cancelar / Guardar
- Botón flecha ← volver (mismo efecto que Cancelar)
- Panel "Ítems de la lista":
  - Cada ítem: drag handle (visual, no funcional) · thumbnail · nombre · tipo · input segundos · btn eliminar ítem
  - Eliminar ítem: `this.parentElement.remove()` inmediato, sin confirm
  - Botón `+ Agregar Ítem` → toast mock "Selector de media próximamente"
- Guardar → vuelve a listado + toast éxito
- Cancelar → vuelve a listado sin toast

---

### 5.5 Media

**Upload zone:**
- Texto: "Haz clic o arrastra archivos multimedia aquí"
- Sub-texto: "Soporta JPG, PNG, MP4 (Máx 50MB)"
- Click → muestra progress bar estática al 45% + texto "Subiendo banner_nuevo.jpg (45%)"
- Después de 1500ms → toast "Archivo subido correctamente" + oculta progress
- Sin validación real de archivo

**Grid biblioteca (auto-fill, min 220px):**
- 6 cards con thumbnails de color (video → rojo, imagen → negro)
- Por card: thumbnail · nombre · tipo + peso + duración · acciones
- **"Ver uso (N)"** → `modal-media-usage`
  - Si N > 0: lista de playlists donde aparece
  - Si N = 0: texto italic "No se está utilizando en ninguna playlist"
- **Eliminar (ícono basura)** → `modal-delete-media`
  - Si está en uso: advertencia roja "Este archivo está en uso en una o más playlists..."
  - Si no está en uso: solo confirm genérico
  - Confirmar → toast éxito

---

### 5.6 Historial

**Filtro:**
- Dropdown: Todas las acciones · Creación · Actualización · Eliminación · Sistema
- Filtro es mock (no filtra realmente en el prototipo)

**Tabla AuditLog:**
- Columnas: Fecha / Hora · Usuario · Acción · Entidad · Detalle
- 10 filas por página, 45 registros totales → 5 páginas
- Acciones con color semántico sugerido (opcional en prototipo):
  - Creación → verde
  - Eliminación → rojo
  - Actualización → azul/neutro
  - Sistema → gris

**Paginación funcional:**
- Info: "Mostrando X - Y de 45 registros"
- Botones: Anterior · 1 · 2 · 3 · 4 · 5 · Siguiente
- Página activa: fondo negro, texto blanco
- Anterior deshabilitado en página 1, Siguiente deshabilitado en página 5
- Click en número/anterior/siguiente → cambia las filas mostradas

---

## 6. Modales

| ID | Título | Trigger | Acciones |
|----|--------|---------|----------|
| `modal-playlist` | Asignar Playlist | Btn "Asignar Playlist" en fila de pantalla | Dropdown playlists · Confirmar (toast) · Cancelar |
| `modal-delete-screen` | Eliminar Pantalla | Btn "Eliminar" en fila de pantalla activa | Confirm simple · Sí Eliminar (toast) · Cancelar |
| `modal-delete-playlist` | Eliminar Playlist | Btn "Eliminar" en fila de playlist | Confirm simple · Sí Eliminar (toast) · Cancelar |
| `modal-delete-media` | Eliminar Archivo | Btn basura en media card | Advertencia condicional si en uso · Sí Eliminar (toast) · Cancelar |
| `modal-media-usage` | Uso en Playlists | Btn "Ver uso" en media card | Lista de playlists o empty state · Cerrar |

**Comportamiento estándar de todos los modales:**
- Overlay `rgba(0,0,0,0.7)`
- Cierre con X o click fuera del container
- Max-width: 450px, max-height: 90vh con scroll interno en body
- Estructura: header · body · footer-actions

---

## 7. Componentes Globales

### Toasts
- Posición: fixed, bottom-right, `z-index: 9999`
- Tipos: `success` (borde izquierdo verde) · `error` (borde izquierdo rojo)
- Animación: slide-in desde derecha (transform translateX)
- Auto-dismiss: 3000ms + fade-out 300ms
- Apilables si se disparan varios

### Badges de estado (pills)
```
Online:  fondo #E8F5E9 · texto #4CAF50 · dot verde
Offline: fondo #FFEBEE · texto #F44336 · dot rojo
```

### Badge de notificación en nav
- Círculo rojo pequeño con número, posicionado sobre ícono de Pantallas
- Visible solo si `pendientes.length > 0`

---

## 8. Decisiones de Negocio Confirmadas

| # | Decisión |
|---|----------|
| Las pantallas NO se crean manualmente desde el admin | El player se auto-registra y aparece en cola |
| Device ID auto-generado | Formato `DEVICE-XXXX` (4 caracteres alfanuméricos) |
| Rechazar dispositivo | Solo desaparece de la cola, sin log de rechazados |
| Eliminar pantalla activa | Sí permitido, confirm simple sin advertencias adicionales |
| Asignar playlist a pantalla offline | Permitido, sin validación de estado |
| Cuota de almacenamiento | No visible en esta versión |
| Un solo usuario admin | Sin gestión de usuarios |
| Sin Schedule | Playlists no tienen horario programado |
| Sin Analytics | No hay métricas de reproducción |

---

## 9. Fuera de Scope (v1.0)

- Gestión de usuarios / roles
- Scheduling / programación por horario
- Analytics de reproducción
- Preview de playlist antes de publicar
- Notificaciones push / email
- Responsive / mobile
- Autenticación real (JWT, sesiones)
- Backend / persistencia

---

*Seed generado post-VoBo · Sichi Digital Signage Admin v1.0*
