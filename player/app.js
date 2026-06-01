(function () {
  const DEFAULT_API_URL = 'http://api.sichi.com';
  const APPROVAL_POLL_MS = 10_000;

  const { initDB, getConfig, setConfig, getPlaylist } = window.SichiDB;
  const { syncPlaylist } = window.SichiPlaylist;
  const { start: startRenderer } = window.SichiRenderer;
  const { start: startHeartbeat } = window.SichiHeartbeat;
  const { connectSignalR } = window.SichiSignalR;

  let approvalPollTimer = null;

  function getQueryParam(name) {
    return new URLSearchParams(window.location.search).get(name);
  }

  function resolveApiUrl() {
    const fromQuery = getQueryParam('api');
    return (fromQuery || DEFAULT_API_URL).replace(/\/$/, '');
  }

  function showWaitingScreen(message) {
    const waiting = document.getElementById('waiting-screen');
    const error = document.getElementById('error-screen');
    const statusText = document.getElementById('status-text');

    error.style.display = 'none';
    error.classList.add('hidden');
    waiting.classList.remove('hidden');
    waiting.style.display = 'flex';
    if (message) {
      statusText.textContent = message;
    }
  }

  function hideWaitingScreen() {
    const waiting = document.getElementById('waiting-screen');
    waiting.classList.add('hidden');
    waiting.style.display = 'none';
  }

  function showErrorScreen(message) {
    const waiting = document.getElementById('waiting-screen');
    const error = document.getElementById('error-screen');
    const errorText = document.getElementById('error-text');

    waiting.classList.add('hidden');
    waiting.style.display = 'none';
    error.classList.remove('hidden');
    error.style.display = 'flex';
    errorText.textContent = message;
  }

  function stopApprovalPoll() {
    if (approvalPollTimer !== null) {
      clearInterval(approvalPollTimer);
      approvalPollTimer = null;
    }
  }

  async function registerScreen(screenKey) {
    const apiUrl = await getConfig('apiUrl');
    const response = await fetch(`${apiUrl}/api/player/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ screenKey }),
    });

    if (!response.ok) {
      throw new Error(`Register failed: ${response.status}`);
    }

    return response.json();
  }

  async function waitForApproval(screenKey) {
    showWaitingScreen('Esperando aprobación del administrador...');

    const poll = async () => {
      try {
        const result = await registerScreen(screenKey);
        if (result.isApproved) {
          stopApprovalPoll();
          return true;
        }
      } catch (err) {
        console.warn('[SichiApp] Approval poll error:', err);
      }
      return false;
    };

    if (await poll()) return;

    return new Promise((resolve) => {
      approvalPollTimer = setInterval(async () => {
        if (await poll()) {
          resolve();
        }
      }, APPROVAL_POLL_MS);
    });
  }

  async function fetchRemoteVersion(screenKey) {
    const apiUrl = await getConfig('apiUrl');
    const response = await fetch(
      `${apiUrl}/api/player/version?screen=${encodeURIComponent(screenKey)}`
    );

    if (!response.ok) {
      throw new Error(`Version fetch failed: ${response.status}`);
    }

    const data = await response.json();
    return data.version ?? 0;
  }

  async function tryStartPlayback(screenKey, statusMessage) {
    const playlist = await getPlaylist();

    if (!playlist || !playlist.items || playlist.items.length === 0) {
      showWaitingScreen(statusMessage || 'Cargando contenido...');
      return false;
    }

    try {
      hideWaitingScreen();
      await startRenderer();
      startHeartbeat(screenKey);
      return true;
    } catch (err) {
      console.error('[SichiApp] Renderer start failed:', err);
      showWaitingScreen('Cargando contenido...');
      return false;
    }
  }

  async function runOnlineFlow(screenKey) {
    try {
      await connectSignalR(screenKey);
    } catch (err) {
      console.warn('[SichiApp] SignalR connection failed:', err);
    }

    const remoteVersion = await fetchRemoteVersion(screenKey);
    const localVersion = (await getConfig('lastKnownVersion')) ?? 0;

    if (remoteVersion > localVersion) {
      showWaitingScreen('Sincronizando contenido...');
      await syncPlaylist(screenKey);
    }

    const started = await tryStartPlayback(screenKey);
    if (!started) {
      showWaitingScreen('Cargando contenido...');
    }
  }

  async function runOfflineFlow(screenKey) {
    showWaitingScreen('Sin conexión — reproduciendo desde caché');
    const started = await tryStartPlayback(
      screenKey,
      'Sin conexión — reproduciendo desde caché'
    );
    if (!started) {
      showWaitingScreen('Cargando contenido...');
    }
  }

  async function bootstrap() {
    const screenKey = getQueryParam('screen');

    if (!screenKey) {
      showErrorScreen('Falta parámetro ?screen=');
      return;
    }

    const apiUrl = resolveApiUrl();

    try {
      await initDB();
      await setConfig('screenKey', screenKey);
      await setConfig('apiUrl', apiUrl);
    } catch (err) {
      console.error('[SichiApp] IndexedDB init failed:', err);
      showErrorScreen('Error al inicializar almacenamiento local');
      return;
    }

    showWaitingScreen('Cargando contenido...');

    try {
      const registerResult = await registerScreen(screenKey);

      if (!registerResult.isApproved) {
        await waitForApproval(screenKey);
      }

      await runOnlineFlow(screenKey);
    } catch (err) {
      console.warn('[SichiApp] Online flow failed, trying offline:', err);
      stopApprovalPoll();
      await runOfflineFlow(screenKey);
    }
  }

  window.SichiApp = {
    showWaitingScreen,
    hideWaitingScreen,
    showErrorScreen,
    bootstrap,
  };

  bootstrap();
})();
