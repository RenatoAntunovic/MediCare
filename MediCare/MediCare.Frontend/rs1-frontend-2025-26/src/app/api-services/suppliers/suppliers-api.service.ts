import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { buildHttpParams } from '../../core/models/build-http-params';
import {
  GetSupplierByIdQueryDto,
  ListSuppliersRequest,
  ListSuppliersResponse,
  UpsertSupplierCommand
} from './suppliers-api.models';

@Injectable({
  providedIn: 'root',
})
export class SuppliersApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/suppliers`;
  private http = inject(HttpClient);

  list(request?: ListSuppliersRequest): Observable<ListSuppliersResponse> {
    const params = request ? buildHttpParams(request as any) : undefined;
    return this.http.get<ListSuppliersResponse>(this.baseUrl, { params });
  }

  /** Distinct cities for the city filter. */
  cities(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/cities`);
  }

  getById(id: number): Observable<GetSupplierByIdQueryDto> {
    return this.http.get<GetSupplierByIdQueryDto>(`${this.baseUrl}/${id}`);
  }

  create(command: UpsertSupplierCommand): Observable<{ id: number }> {
    return this.http.post<{ id: number }>(this.baseUrl, command);
  }

  update(id: number, command: UpsertSupplierCommand): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, command);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  enable(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/enable`, {});
  }

  disable(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/disable`, {});
  }
}
