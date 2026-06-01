# Sichi Digital Signage — Despliegue local

## 1. Requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) en Windows
- [Git](https://git-scm.com/)

## 2. Primer arranque

```bash
cp .env.example .env
```

Edita `.env` con valores reales (contraseñas, secretos JWT, etc.).

```bash
docker compose up --build -d
```

## 3. Verificar servicios

```bash
docker compose ps
```

Todos los servicios (`nginx`, `api`, `mysql`) deben aparecer en estado `running`.

## 4. Archivo hosts de Windows

Agrega estas entradas en `C:\Windows\System32\drivers\etc\hosts` (como administrador):

```
127.0.0.1  admin.sichi.com
127.0.0.1  player.sichi.com
127.0.0.1  api.sichi.com
```

## 5. Smoke test

Cuando la API esté disponible (Node 4.2):

```bash
curl http://api.sichi.com/api/health
```

Debe responder con estado OK.
