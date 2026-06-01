(function () {
  const DB_NAME = 'sichi_player';
  const DB_VERSION = 1;

  let dbPromise = null;

  function openDatabase() {
    if (dbPromise) return dbPromise;

    dbPromise = new Promise((resolve, reject) => {
      const request = indexedDB.open(DB_NAME, DB_VERSION);

      request.onerror = () => reject(request.error);

      request.onupgradeneeded = (event) => {
        const db = event.target.result;

        if (!db.objectStoreNames.contains('config')) {
          db.createObjectStore('config', { keyPath: 'key' });
        }

        if (!db.objectStoreNames.contains('playlist')) {
          db.createObjectStore('playlist', { keyPath: 'id' });
        }

        if (!db.objectStoreNames.contains('media_blobs')) {
          db.createObjectStore('media_blobs', { keyPath: 'mediaItemId' });
        }
      };

      request.onsuccess = () => resolve(request.result);
    });

    return dbPromise;
  }

  function runTransaction(storeName, mode, fn) {
    return openDatabase().then(
      (db) =>
        new Promise((resolve, reject) => {
          const tx = db.transaction(storeName, mode);
          const store = tx.objectStore(storeName);
          const result = fn(store);

          tx.oncomplete = () => resolve(result);
          tx.onerror = () => reject(tx.error);
          tx.onabort = () => reject(tx.error);
        })
    );
  }

  async function initDB() {
    await openDatabase();
  }

  async function getConfig(key) {
    return runTransaction('config', 'readonly', (store) => {
      return new Promise((resolve, reject) => {
        const request = store.get(key);
        request.onsuccess = () => resolve(request.result?.value ?? null);
        request.onerror = () => reject(request.error);
      });
    });
  }

  async function setConfig(key, value) {
    return runTransaction('config', 'readwrite', (store) => {
      store.put({ key, value });
    });
  }

  async function savePlaylist(playlist) {
    return runTransaction('playlist', 'readwrite', (store) => {
      store.put(playlist);
    });
  }

  async function getPlaylist() {
    return runTransaction('playlist', 'readonly', (store) => {
      return new Promise((resolve, reject) => {
        const request = store.get(1);
        request.onsuccess = () => resolve(request.result ?? null);
        request.onerror = () => reject(request.error);
      });
    });
  }

  async function saveBlob(mediaItemId, blob, checksum) {
    return runTransaction('media_blobs', 'readwrite', (store) => {
      store.put({ mediaItemId, blob, checksum });
    });
  }

  async function getBlob(mediaItemId) {
    return runTransaction('media_blobs', 'readonly', (store) => {
      return new Promise((resolve, reject) => {
        const request = store.get(mediaItemId);
        request.onsuccess = () => resolve(request.result ?? null);
        request.onerror = () => reject(request.error);
      });
    });
  }

  async function clearBlobs() {
    return runTransaction('media_blobs', 'readwrite', (store) => {
      store.clear();
    });
  }

  window.SichiDB = {
    initDB,
    getConfig,
    setConfig,
    savePlaylist,
    getPlaylist,
    saveBlob,
    getBlob,
    clearBlobs,
  };
})();
