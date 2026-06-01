(function () {
  const { getConfig, saveBlob } = window.SichiDB;

  async function getApiUrl() {
    return (await getConfig('apiUrl')) || '';
  }

  function isSecureContextForCrypto() {
    return (
      window.isSecureContext &&
      typeof crypto !== 'undefined' &&
      typeof crypto.subtle !== 'undefined'
    );
  }

  async function sha256Hex(arrayBuffer) {
    const hashBuffer = await crypto.subtle.digest('SHA-256', arrayBuffer);
    return [...new Uint8Array(hashBuffer)]
      .map((b) => b.toString(16).padStart(2, '0'))
      .join('');
  }

  async function downloadAndCache({ mediaItemId, filePath, checksum }) {
    const apiUrl = await getApiUrl();
    const url = `${apiUrl.replace(/\/$/, '')}/${filePath.replace(/^\//, '')}`;


    const response = await fetch(url);
    if (!response.ok) {
      throw new Error(`Download failed: ${response.status} ${url}`);
    }

    const arrayBuffer = await response.arrayBuffer();

    if (isSecureContextForCrypto()) {
      const hashHex = await sha256Hex(arrayBuffer);
      if (hashHex !== (checksum || '').toLowerCase()) {
        throw new Error(`Checksum mismatch for mediaItemId ${mediaItemId}`);
      }
      const blob = new Blob([arrayBuffer], { type: 'video/mp4' });
      await saveBlob(mediaItemId, blob, hashHex);
      return;
    }

    console.warn(
      '[SichiCache] SubtleCrypto unavailable (HTTP context). Skipping checksum verification.'
    );
    const blob = new Blob([arrayBuffer], { type: 'video/mp4' });
    await saveBlob(mediaItemId, blob, checksum);
  }

  window.SichiCache = {
    downloadAndCache,
  };
})();
