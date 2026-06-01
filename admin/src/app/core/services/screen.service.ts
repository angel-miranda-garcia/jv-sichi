import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { PendingScreen, Screen } from '../models/screen.model';

@Injectable({ providedIn: 'root' })
export class ScreenService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/screens`;

  getScreens(): Observable<Screen[]> {
    return this.http.get<Screen[]>(this.baseUrl);
  }

  getPending(): Observable<PendingScreen[]> {
    return this.http.get<PendingScreen[]>(`${this.baseUrl}/pending`);
  }

  approve(id: number): Observable<Screen> {
    return this.http.post<Screen>(`${this.baseUrl}/${id}/approve`, {});
  }

  reject(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}/reject`);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  assignPlaylist(id: number, playlistId: number | null): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/playlist`, {
      playlistId,
    });
  }

  forceRefresh(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/force-refresh`, {});
  }
}
