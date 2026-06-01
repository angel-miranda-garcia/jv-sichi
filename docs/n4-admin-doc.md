# Sichi Digital Signage — Angular Admin
## Documentación Técnica · Nodos 4.6 → 4.8
**Estado:** Completado  
**Fecha:** 2026-06-01  
**Stack:** Angular 21 · Standalone Components · Signals · @angular/cdk · @microsoft/signalr

---

## 1. Estructura del Proyecto

```
admin/
├── proxy.conf.json              -- /api y /signalr → localhost:5000
├── src/
│   ├── styles.scss              -- variables globales + clases utilitarias
│   ├── environments/
│   │   ├── environment.ts       -- apiUrl: '' (proxy en dev)
│   │   └── environment.prod.ts  -- apiUrl: 'https://api.sichi.com'
│   └── app/
│       ├── app.config.ts        -- provideRouter, provideHttpClient, provideAnimations
│       ├── app.routes.ts        -- rutas con AuthGuard
│       ├── core/
│       │   ├── auth/            AuthService, AuthGuard, JwtInterceptor
│       │   ├── models/          Screen, Playlist, MediaItem, AuditLog
│       │   └── services/        ScreenService, PlaylistService,
│       │                        MediaService, AuditService, SignalrService
│       ├── layout/
│       │   └── sidebar/         Layout shell colapsable
│       ├── features/
│       │   ├── login/           LoginComponent
│       │   ├── dashboard/       DashboardComponent
│       │   ├── screens/         ScreensComponent + modales
│       │   ├── playlists/       PlaylistsComponent + editor + modales
│       │   ├── media/           MediaComponent + modales
│       │   └── history/         HistoryComponent
│       └── shared/
│           └── components/
│               ├── toast/       ToastService + ToastComponent
│               └── modal-base/  ModalBaseComponent
```

---

## 2. Identidad Visual

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
| Font | Inter (Google Fonts) 400/500/600/700/800 |
| Referencia | `docs/prototype.html` — prototipo aprobado por cliente |

---

## 3. Routing

```
/login              → LoginComponent         (público)
/dashboard          → DashboardComponent     (AuthGuard)
/screens            → ScreensComponent       (AuthGuard)
/playlists          → PlaylistsComponent     (AuthGuard)
/playlists/:id/edit → PlaylistsComponent     (AuthGuard)
/media              → MediaComponent         (AuthGuard)
/history            → HistoryComponent       (AuthGuard)
```

---

## 4. Auth

| Aspecto | Implementación |
|---------|----------------|
| Token storage | Solo en memoria (no localStorage) |
| Expiración | Verificada en `isAuthenticated()` via decode JWT exp |
| Guard | `AuthGuard` funcional (CanActivateFn) |
| Interceptor | `jwtInterceptor` funcional (HttpInterceptorFn) agrega `Bearer` |
| Logout | Limpia token en memoria + navega a `/login` |

---

## 5. Módulos por Feature

### Login
- Formulario reactivo con `FormGroup`
- Signals: `isLoading`, `errorMessage`
- Error state: bloque rojo claro `#FFEBEE`
- Redirect a `/dashboard` tras éxito

### Dashboard
- Métricas: Total / Online / Offline (computed signals)
- Tabla monitoreo en vivo con SignalR
- Botón "Forzar Refresh": `opacity 0.5 + cursor not-allowed` si offline
- `SignalrService`: actualiza pills sin recargar página

### Screens
- Tab **Activas**: tabla con assign playlist, force refresh, eliminar
- Tab **Pendientes**: aprobar / rechazar con badge en sidebar
- `AssignPlaylistModalComponent`: dropdown con playlist actual preseleccionada
- `DeleteScreenModalComponent`: confirm simple
- `formatLastPing()`: tiempo relativo (seg / min / horas)

### Playlists
- Vista **Listado**: tabla con editar / eliminar
- Vista **Editor**: header sticky + drag & drop CDK + input segundos por ítem
- `MediaSelectorModalComponent`: grid de media disponible para agregar
- `DeletePlaylistModalComponent`: confirm simple
- Save: crear → addMedia secuencial; editar → reorder

### Media
- Upload zone: drag & drop nativo + click
- Progress bar real con `HttpEventType.UploadProgress`
- Grid biblioteca: cards con thumbnail rojo
- `MediaUsageModalComponent`: lista playlists o empty state italic
- `DeleteMediaModalComponent`: advertencia roja si archivo en uso
- Manejo de errores: 400 archivo inválido, 429 rate limit

### Historial
- Tabla AuditLog paginada (10 registros/página)
- Paginación: Anterior · números · Siguiente
- Info: "Mostrando X - Y de N registros"
- `getActionColor()`: verde/rojo/azul/gris por tipo de acción
- Filtro por acción (mock — no filtra en API)

---

## 6. Servicios Core

### AuthService
```typescript
login(email, password): Observable<void>
logout(): void
getToken(): string | null
isAuthenticated(): boolean   // verifica exp del JWT
getUserId(): number | null
getUserEmail(): string | null
```

### SignalrService
```typescript
startConnection(): void      // idempotente
stopConnection(): void
screenUpdated$: Observable<Screen>
```
Conecta a `/signalr/player`, escucha `screenStatusUpdated`.
Usado en `DashboardComponent` (ngOnInit / ngOnDestroy).

### ToastService
```typescript
show(message, type?: 'success' | 'error'): void
dismiss(id): void
```
Signal-based, auto-dismiss 3000ms, apilable, posición fixed bottom-right.

---

## 7. Componentes Compartidos

### ModalBaseComponent
- Input: `title`, `isOpen`
- Output: `closed`
- Overlay `rgba(0,0,0,0.7)`, cierra con X o click fuera
- `ng-content` slots: `[modal-body]`, `[modal-footer]`
- Max-width 450px, scroll interno en body

### ToastComponent
- Slide-in desde derecha (translateX)
- Borde izquierdo: verde (success) / rojo (error)
- Auto-dismiss + click para cerrar

---

## 8. Sidebar

| Estado | Width |
|--------|-------|
| Expandido | 260px |
| Colapsado | 80px |

- Badge rojo en "Pantallas" si `pendingCount > 0`
- Se refresca en cada navegación (Router events)
- Muestra email del admin en footer
- Botón logout

---

## 9. Decisiones Técnicas

| # | Decisión |
|---|----------|
| Standalone components | Sin NgModules en toda la app |
| Signals | Estado local con `signal()` y `computed()` |
| JWT en memoria | No persistido en localStorage |
| CDK DragDrop | Para reorder de ítems en playlist editor |
| SignalR | `@microsoft/signalr ^8.0.7` |
| Proxy dev | `/api` y `/signalr` → `localhost:5000` |
| Filtro historial | Mock en frontend, sin parámetro al API |
| Desktop first | Sin breakpoints responsive en v1.0 |

---

## 10. Notas de Construcción

| Nodo | Commit |
|------|--------|
| 4.6-a | `feat(admin): angular scaffolding + models + services + routing` |
| 4.6-b | `feat(admin): auth service + JWT interceptor + sidebar layout + login` |
| 4.7-a | `feat(admin): screens module with tabs, approval flow and modals` |
| 4.7-b | `feat(admin): playlists module with editor and CDK drag & drop` |
| 4.8-a | `feat(admin): media module with upload progress and usage modals` |
| 4.8-b | `feat(admin): dashboard live monitoring + SignalR + audit history` |

> **Nota:** Node 4.6-a fue commiteado con el mensaje de 4.6-b por error.
> El fix se aplicó corriendo 4.6-b con commit:
> `fix(admin): replace 4.6-a placeholders with full 4.6-b implementation`

---

## 11. Fuera de Scope (v1.0)

- Responsive / mobile
- Autenticación real con refresh token
- Persistencia de JWT entre recargas
- Preview de playlist antes de publicar
- Notificaciones push / email
- Analytics de reproducción
- Scheduling por horario

---

*Documento generado post Node 4.8 · Sichi Digital Signage Angular Admin*
