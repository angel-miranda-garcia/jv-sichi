(function () {
  const { getConfig } = window.SichiDB;

  let intervalId = null;
  let screenKeyRef = null;

  async function getApiUrl() {
    return (await getConfig('apiUrl')) || '';
  }

  async function ping() {
    if (!screenKeyRef) return;

    try {
      const apiUrl = await getApiUrl();
      await fetch(`${apiUrl.replace(/\/$/, '')}/api/player/heartbeat`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          screenKey: screenKeyRef,
          timestamp: new Date().toISOString(),
        }),
      });
    } catch {
      // Silenciar errores de red
    }
  }

  function start(screenKey) {
    screenKeyRef = screenKey;
    ping();
    if (intervalId !== null) {
      clearInterval(intervalId);
    }
    intervalId = setInterval(ping, 30_000);
  }

  function stop() {
    if (intervalId !== null) {
      clearInterval(intervalId);
      intervalId = null;
    }
    screenKeyRef = null;
  }

  window.SichiHeartbeat = {
    start,
    stop,
    ping,
  };
})();
