import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuditLogPage } from '../models/audit-log.model';

@Injectable({ providedIn: 'root' })
export class AuditService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/audit`;

  getAll(page: number, pageSize: number): Observable<AuditLogPage> {
    return this.http.get<AuditLogPage>(this.baseUrl, {
      params: { page: String(page), pageSize: String(pageSize) },
    });
  }
}
