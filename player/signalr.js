(function () {
  const { getConfig } = window.SichiDB;
  const { syncPlaylist } = window.SichiPlaylist;
  const { reload } = window.SichiRenderer;

  let connection = null;

  async function getApiUrl() {
    return (await getConfig('apiUrl')) || '';
  }

  async function connectSignalR(screenKey) {
    const apiUrl = await getApiUrl();
    const hubUrl = `${apiUrl.replace(/\/$/, '')}/signalr/player`;

    connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        transport:
          signalR.HttpTransportType.WebSockets |
          signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();

    connection.on('playlistChanged', async ({ version }) => {
      try {
        const localVersion = (await getConfig('lastKnownVersion')) ?? 0;
        if (version > localVersion) {
          await syncPlaylist(screenKey);
          await reload();
        }
      } catch (err) {
        console.warn('[SichiSignalR] playlistChanged handler failed:', err);
      }
    });

    connection.on('forceRefresh', async () => {
      try {
        await syncPlaylist(screenKey);
        await reload();
      } catch (err) {
        console.warn('[SichiSignalR] forceRefresh handler failed:', err);
      }
    });

    await connection.start();
    await connection.invoke('ScreenConnected', screenKey);
  }

  function disconnect() {
    if (connection) {
      connection.stop().catch(() => {});
      connection = null;
    }
  }

  window.SichiSignalR = {
    connectSignalR,
    disconnect,
  };
})();
