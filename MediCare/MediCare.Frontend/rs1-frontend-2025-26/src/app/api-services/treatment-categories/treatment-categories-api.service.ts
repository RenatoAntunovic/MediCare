import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ListTreatmentCategoriesRequest,
  ListTreatmentCategoriesResponse,
  GetTreatmentCategoryByIdQueryDto,
  CreateTreatmentCategoryCommand,
  UpdateTreatmentCategoryCommand
} from './treatment-categories-api.model';
import { buildHttpParams } from '../../core/models/build-http-params';

@Injectable({
  providedIn: 'root',
})
export class TreatmentCategoriesApiService {
  private readonly baseUrl = `${environment.apiUrl}/TreatmentCategories`;
  private http = inject(HttpClient);

  /**
   * GET /ProductCategories
   * List categories with optional query parameters.
   */
  list(request?: ListTreatmentCategoriesRequest): Observable<ListTreatmentCategoriesResponse> {
    const params = request ? buildHttpParams(request as any) : undefined;

    return this.http.get<ListTreatmentCategoriesResponse>(this.baseUrl, {
      params,
    });
  }

  /**
   * GET /ProductCategories/{id}
   * Get a single category by ID.
   */
  getById(id: number): Observable<GetTreatmentCategoryByIdQueryDto> {
    return this.http.get<GetTreatmentCategoryByIdQueryDto>(`${this.baseUrl}/${id}`);
  }

  /**
   * POST /ProductCategories
   * Create a new category.
   * @returns ID of the newly created category
   */
  create(payload: CreateTreatmentCategoryCommand): Observable<number> {
    return this.http.post<number>(this.baseUrl, payload);
  }

  /**
   * PUT /ProductCategories/{id}
   * Update an existing category.
   */
  update(id: number, payload: UpdateTreatmentCategoryCommand): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  /**
   * DELETE /ProductCategories/{id}
   * Delete a category.
   */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  /**
   * PUT /MedicineCategories/{id}/disable
   * Disable a category.
   */
  disable(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/disable`, {});
  }

  /**
   * PUT /MedicineCategories/{id}/enable
   * Enable a category.
   */
  enable(id: number): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/enable`, {});
  }
}
