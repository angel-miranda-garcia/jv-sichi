import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  Playlist,
  PlaylistDetail,
} from '../models/playlist.model';

@Injectable({ providedIn: 'root' })
export class PlaylistService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/playlists`;

  getAll(): Observable<Playlist[]> {
    return this.http.get<Playlist[]>(this.baseUrl);
  }

  getById(id: number): Observable<PlaylistDetail> {
    return this.http.get<PlaylistDetail>(`${this.baseUrl}/${id}`);
  }

  create(name: string): Observable<Playlist> {
    return this.http.post<Playlist>(this.baseUrl, { name });
  }

  update(id: number, name: string): Observable<Playlist> {
    return this.http.put<Playlist>(`${this.baseUrl}/${id}`, { name });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  reorder(
    id: number,
    items: { mediaItemId: number; sortOrder: number }[]
  ): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/reorder`, items);
  }

  addMedia(
    id: number,
    mediaItemId: number,
    sortOrder: number,
    overrideDuration?: number
  ): Observable<void> {
    const body: {
      mediaItemId: number;
      sortOrder: number;
      overrideDuration?: number;
    } = { mediaItemId, sortOrder };
    if (overrideDuration !== undefined) {
      body.overrideDuration = overrideDuration;
    }
    return this.http.post<void>(`${this.baseUrl}/${id}/media`, body);
  }

  removeMedia(id: number, mediaItemId: number): Observable<void> {
    return this.http.delete<void>(
      `${this.baseUrl}/${id}/media/${mediaItemId}`
    );
  }
}
