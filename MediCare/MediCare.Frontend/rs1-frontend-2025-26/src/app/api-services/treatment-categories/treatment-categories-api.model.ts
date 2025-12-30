import { BasePagedQuery } from '../../core/models/paging/base-paged-query';
import { PageResult } from '../../core/models/paging/page-result';

// === QUERIES (READ) ===

/**
 * Query parameters for GET /TreatmentCategories
 * Corresponds to: ListTreatmentCategoriesQuery.cs
 */
export class ListTreatmentCategoriesRequest extends BasePagedQuery {
  search?: string | null;
  onlyEnabled?: boolean | null;
}

/**
 * Response item for GET /TreatmentCategories
 * Corresponds to: ListTreatmentCategoriesQueryDto.cs
 */
export interface ListTreatmentCategoriesQueryDto {
  id: number;
  categoryName: string;
  isEnabled: boolean;
}

/**
 * Response for GET /TreatmentCategories/{id}
 * Corresponds to: GetTreatmentCategoryByIdQueryDto.cs
 */
export interface GetTreatmentCategoryByIdQueryDto {
  id: number;
  categoryName: string;
  isEnabled: boolean;
}

/**
 * Paged response for GET /TreatmentCategories
 */
export type ListTreatmentCategoriesResponse = PageResult<ListTreatmentCategoriesQueryDto>;

// === COMMANDS (WRITE) ===

/**
 * Command for POST /TreatmentCategories
 * Corresponds to: CreateTreatmentCategoryCommand.cs
 */
export interface CreateTreatmentCategoryCommand {
  name: string;
}

/**
 * Command for PUT /TreatmentCategories/{id}
 * Corresponds to: UpdateTreatmenttCategoryCommand.cs
 */
export interface UpdateTreatmentCategoryCommand {
  name: string;
}
