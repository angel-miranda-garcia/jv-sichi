export interface Screen {
  id: number;
  name: string;
  screenKey: string;
  deviceId: string;
  isOnline: boolean;
  isApproved: boolean;
  currentPlaylistId: number | null;
  currentPlaylistName: string | null;
  lastPing: string | null;
  createdAt: string;
}

export interface PendingScreen {
  id: number;
  deviceId: string;
  screenKey: string;
  createdAt: string;
}
