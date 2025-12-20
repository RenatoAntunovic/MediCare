import { BasePagedQuery } from '../../core/models/paging/base-paged-query';
import { PageResult } from '../../core/models/paging/page-result';

// === QUERIES (READ) ===

/**
 * Query parameters for GET /MedicineCategories
 * Corresponds to: ListMedicineCategoriesQuery.cs
 */
export class ListMedicineCategoriesRequest extends BasePagedQuery {
  search?: string | null;
  onlyEnabled?: boolean | null;
}

/**
 * Response item for GET /MedicineCategories
 * Corresponds to: ListMedicineCategoriesQueryDto.cs
 */
export interface ListMedicineCategoriesQueryDto {
  id: number;
  name: string;
  isEnabled: boolean;
}

/**
 * Response for GET /MedicineCategories/{id}
 * Corresponds to: GetMedicineCategoryByIdQueryDto.cs
 */
export interface GetMedicineCategoryByIdQueryDto {
  id: number;
  name: string;
  isEnabled: boolean;
}

/**
 * Paged response for GET /MedicineCategories
 */
export type ListMedicineCategoriesResponse = PageResult<ListMedicineCategoriesQueryDto>;

// === COMMANDS (WRITE) ===

/**
 * Command for POST /MedicineCategories
 * Corresponds to: CreateMedicineCategoryCommand.cs
 */
export interface CreateMedicineCategoryCommand {
  name: string;
}

/**
 * Command for PUT /MedicineCategories/{id}
 * Corresponds to: UpdateMedicinetCategoryCommand.cs
 */
export interface UpdateMedicineCategoryCommand {
  name: string;
}
