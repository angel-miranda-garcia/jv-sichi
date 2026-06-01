(function () {
  const { getConfig, setConfig, savePlaylist, getBlob } = window.SichiDB;
  const { downloadAndCache } = window.SichiCache;

  async function getApiUrl() {
    return (await getConfig('apiUrl')) || '';
  }

  async function fetchPlaylist(screenKey) {
    const apiUrl = await getApiUrl();
    const url = `${apiUrl.replace(/\/$/, '')}/api/player/playlist?screen=${encodeURIComponent(screenKey)}`;
    const response = await fetch(url);

    if (!response.ok) {
      throw new Error(`Playlist fetch failed: ${response.status}`);
    }

    return response.json();
  }

  async function syncPlaylist(screenKey) {
    const data = await fetchPlaylist(screenKey);
    const version = data.version;
    const items = [...(data.items || [])].sort(
      (a, b) => (a.sortOrder ?? 0) - (b.sortOrder ?? 0)
    );

    for (const item of items) {
      const blobRecord = await getBlob(item.mediaItemId);

      const storedChecksum = (blobRecord?.checksum || '').toLowerCase();
      const itemChecksum = (item.checksum || '').toLowerCase();
      if (blobRecord && storedChecksum === itemChecksum) {
        continue;
      }

      await downloadAndCache({
        mediaItemId: item.mediaItemId,
        filePath: item.filePath,
        checksum: item.checksum,
      });
    }

    const playlistItems = items.map((item) => ({
      mediaItemId: item.mediaItemId,
      filePath: item.filePath,
      checksum: item.checksum,
      duration: item.duration ?? item.overrideDuration ?? 10,
      overrideDuration: item.overrideDuration ?? null,
    }));

    await savePlaylist({
      id: 1,
      version,
      items: playlistItems,
    });
    await setConfig('lastKnownVersion', version);
  }

  window.SichiPlaylist = {
    syncPlaylist,
  };
})();
