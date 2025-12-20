import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ListMedicineCategoriesRequest,
  ListMedicineCategoriesResponse,
  GetMedicineCategoryByIdQueryDto,
  CreateMedicineCategoryCommand,
  UpdateMedicineCategoryCommand
} from './medicine-categories-api.model';
import { buildHttpParams } from '../../core/models/build-http-params';

@Injectable({
  providedIn: 'root',
})
export class MedicineCategoriesApiService {
  private readonly baseUrl = `${environment.apiUrl}/MedicineCategories`;
  private http = inject(HttpClient);

  /**
   * GET /ProductCategories
   * List categories with optional query parameters.
   */
  list(request?: ListMedicineCategoriesRequest): Observable<ListMedicineCategoriesResponse> {
    const params = request ? buildHttpParams(request as any) : undefined;

    return this.http.get<ListMedicineCategoriesResponse>(this.baseUrl, {
      params,
    });
  }

  /**
   * GET /ProductCategories/{id}
   * Get a single category by ID.
   */
  getById(id: number): Observable<GetMedicineCategoryByIdQueryDto> {
    return this.http.get<GetMedicineCategoryByIdQueryDto>(`${this.baseUrl}/${id}`);
  }

  /**
   * POST /ProductCategories
   * Create a new category.
   * @returns ID of the newly created category
   */
  create(payload: CreateMedicineCategoryCommand): Observable<number> {
    return this.http.post<number>(this.baseUrl, payload);
  }

  /**
   * PUT /ProductCategories/{id}
   * Update an existing category.
   */
  update(id: number, payload: UpdateMedicineCategoryCommand): Observable<void> {
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
