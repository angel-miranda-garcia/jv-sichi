export interface MediaItem {
  id: number;
  fileName: string;
  filePath: string;
  mediaType: string;
  durationSeconds: number;
  checksum: string;
  fileSize: number;
  createdAt: string;
}

export interface PlaylistUsage {
  playlistId: number;
  playlistName: string;
}
