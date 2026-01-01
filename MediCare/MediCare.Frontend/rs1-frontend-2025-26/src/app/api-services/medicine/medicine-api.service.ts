import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ListMedicineRequest,
  ListMedicineResponse,
  GetMedicineByIdQueryDto,
  CreateMedicineCommand,
  UpdateMedicineCommand,
  GetMedicineByCategoryIdQueryDto
} from './medicine-api.models';
import { buildHttpParams } from '../../core/models/build-http-params';

@Injectable({
  providedIn: 'root'
})
export class MedicineApiService {
  private readonly baseUrl = `${environment.apiUrl}/api/Medicine`;
  private http = inject(HttpClient);

  /**
   * GET /Products
   * List products with optional query parameters.
   */
  list(request?: ListMedicineRequest): Observable<ListMedicineResponse> {
    const params = request ? buildHttpParams(request as any) : undefined;

    return this.http.get<ListMedicineResponse>(this.baseUrl, {
      params,
    });
  }

  /**
   * GET /Products/{id}
   * Get a single product by ID.
   */
  getById(id: number): Observable<GetMedicineByIdQueryDto> {
    return this.http.get<GetMedicineByIdQueryDto>(`${this.baseUrl}/${id}`);
  }

getByCategoryId(id: number): Observable<GetMedicineByCategoryIdQueryDto[]> {
  return this.http.get<GetMedicineByCategoryIdQueryDto[]>(
    `${this.baseUrl}/by-category/${id}`
  );
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
  return this.http.put(`/api/Medicine/${id}`, data);
}

updateFormData(id: number, formData: FormData): Observable<void> {
  return this.http.put<void>(`/api/Medicine/${id}`, formData);
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

    /**
   * ELASTICSEARCH SEARCH
   */
  searchMedicines(query: string, page: number = 1, pageSize: number = 10): Observable<any> {
    const params = buildHttpParams({
      query: query,
      page: page,
      pageSize: pageSize
    } as any);

    return this.http.get(`${environment.apiUrl}/api/search`, { params });
  }
}


