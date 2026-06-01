import { HttpClient, HttpEvent } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { MediaItem, PlaylistUsage } from '../models/media-item.model';

@Injectable({ providedIn: 'root' })
export class MediaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/media`;

  getAll(): Observable<MediaItem[]> {
    return this.http.get<MediaItem[]>(this.baseUrl);
  }

  upload(file: File): Observable<HttpEvent<MediaItem>> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<MediaItem>(`${this.baseUrl}/upload`, formData, {
      reportProgress: true,
      observe: 'events',
    });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getUsage(id: number): Observable<PlaylistUsage[]> {
    return this.http.get<PlaylistUsage[]>(`${this.baseUrl}/${id}/usage`);
  }
}
