export interface Playlist {
  id: number;
  name: string;
  version: number;
  isActive: boolean;
  itemCount: number;
  totalDuration: string;
  createdAt: string;
}

export interface PlaylistDetail extends Playlist {
  items: PlaylistMediaItem[];
}

export interface PlaylistMediaItem {
  mediaItemId: number;
  fileName: string;
  filePath: string;
  sortOrder: number;
  duration: number;
  overrideDuration: number | null;
}
