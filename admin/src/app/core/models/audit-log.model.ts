export interface AuditLog {
  id: number;
  action: string;
  userEmail: string;
  entityId: number;
  detail: string;
  createdAt: string;
}

export interface AuditLogPage {
  total: number;
  page: number;
  pageSize: number;
  items: AuditLog[];
}
