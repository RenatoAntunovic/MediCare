import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ListTreatmentsRequest,
  ListTreatmentsResponse,
  GetTreatmentsByIdQueryDto,
  CreateTreatmentsCommand,
  UpdateTreatmentsCommand
} from './treatments-api.models';
import { buildHttpParams } from '../../core/models/build-http-params';

@Injectable({
  providedIn: 'root'
})
export class TreatmentsApiService {
  private readonly baseUrl = 'https://localhost:7260/api/Treatments';
  private http = inject(HttpClient);

  /**
   * GET /Products
   * List products with optional query parameters.
   */
  list(request?: ListTreatmentsRequest): Observable<ListTreatmentsResponse> {
    const params = request ? buildHttpParams(request as any) : undefined;

    return this.http.get<ListTreatmentsResponse>(this.baseUrl, {
      params,
    });
  }

  /**
   * GET /Products/{id}
   * Get a single product by ID.
   */
  getById(id: number): Observable<GetTreatmentsByIdQueryDto> {
    return this.http.get<GetTreatmentsByIdQueryDto>(`${this.baseUrl}/${id}`);
  }

  /**
   * POST /Products
   * Create a new product.
   * @returns ID of the newly created product
   */
create(data: FormData) {
  return this.http.post(this.baseUrl, data);
}

createFormData(formData: FormData) {
  return this.http.post<number>(this.baseUrl, formData);
}
  /**
   * PUT /Products/{id}
   * Update an existing product.
   */
  update(id: number, data: FormData) {
  return this.http.put(`/api/Treatments/${id}`, data);
}

updateFormData(id: number, formData: FormData): Observable<void> {
  return this.http.put<void>(`/api/Treatments/${id}`, formData);
}

  /**
   * DELETE /Products/{id}
   * Delete a product.
   */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

    disable(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/disable`, {});
  }

    enable(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/enable`, {});
  }
}
