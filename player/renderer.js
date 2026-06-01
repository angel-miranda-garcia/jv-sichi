(function () {
  const { getPlaylist, getBlob } = window.SichiDB;

  const video = document.getElementById('sichi-player');
  let items = [];
  let currentIndex = 0;
  let advanceTimer = null;

  function clearAdvanceTimer() {
    if (advanceTimer !== null) {
      clearTimeout(advanceTimer);
      advanceTimer = null;
    }
  }

  function revokeObjectUrls() {
    for (const item of items) {
      if (item.objectUrl) {
        URL.revokeObjectURL(item.objectUrl);
      }
    }
  }

  function scheduleAdvance(durationSeconds) {
    clearAdvanceTimer();
    const ms = Math.max(1, durationSeconds) * 1000;
    advanceTimer = setTimeout(() => {
      if (items.length === 0) return;
      playItem((currentIndex + 1) % items.length);
    }, ms);
  }

  function playItem(index) {
    if (items.length === 0) return;

    currentIndex = index;
    const item = items[index];
    clearAdvanceTimer();

    video.src = item.objectUrl;

    const playPromise = video.play();
    if (playPromise && typeof playPromise.catch === 'function') {
      playPromise.catch((err) => {
        console.warn('[SichiRenderer] play() rejected:', err);
      });
    }

    const duration = item.overrideDuration ?? item.duration ?? 10;
    scheduleAdvance(duration);
  }

  async function start() {
    const playlist = await getPlaylist();

    if (!playlist || !playlist.items || playlist.items.length === 0) {
      throw new Error('No playlist items available');
    }

    stop();

    for (const item of playlist.items) {
      const blobRecord = await getBlob(item.mediaItemId);
      if (!blobRecord || !blobRecord.blob) {
        throw new Error(`Missing blob for mediaItemId ${item.mediaItemId}`);
      }

      const objectUrl = URL.createObjectURL(blobRecord.blob);
      items.push({ ...item, objectUrl });
    }

    document.getElementById('waiting-screen').classList.add('hidden');
    document.getElementById('error-screen').classList.add('hidden');
    video.classList.add('visible');
    video.style.display = 'block';

    playItem(0);
  }

  function stop() {
    clearAdvanceTimer();
    video.pause();
    video.removeAttribute('src');
    video.load();
    revokeObjectUrls();
    items = [];
    currentIndex = 0;
    video.classList.remove('visible');
    video.style.display = 'none';
  }

  async function reload() {
    stop();
    await start();
  }

  window.SichiRenderer = {
    start,
    stop,
    reload,
  };
})();
