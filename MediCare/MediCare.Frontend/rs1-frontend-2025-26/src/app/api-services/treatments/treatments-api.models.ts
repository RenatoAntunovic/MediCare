import { PageResult } from '../../core/models/paging/page-result';
import { BasePagedQuery } from '../../core/models/paging/base-paged-query';

// === QUERIES (READ) ===

/**
 * Query parameters for GET /Products
 * Corresponds to: ListProductsQuery.cs
 */
export class ListTreatmentsRequest extends BasePagedQuery {
  search?: string | null;
  // Future filters: categoryId?, isEnabled?, priceMin?, priceMax?
}

/**
 * Response item for GET /Products
 * Corresponds to: ListProductsQueryDto.cs
 */
export interface ListTreatmentsQueryDto {
  id: number;
  serviceName: string;
  description: string;
  price: number;
  treatmentCategoryId: number;
  categoryName: string;
  imagePath: string;
  isEnabled: boolean;
}


/**
 * Response for GET /Products/{id}
 * Corresponds to: GetProductByIdQueryDto.cs
 */
export interface GetTreatmentsByIdQueryDto {
  id: number;
  serviceName: string;
  description: string;
  price: number;
  treatmentsCategoryId: number;
  categoryName: string;
  imagePath: string;
  isEnabled: boolean;
}


/**
 * Paged response for GET /Products
 */
export type ListTreatmentsResponse = PageResult<ListTreatmentsQueryDto>;

// === COMMANDS (WRITE) ===

/**
 * Command for POST /Products
 * Corresponds to: CreateProductCommand.cs
 */
export interface CreateTreatmentsCommand {
  id: number;
  serviceName: string;
  description: string;
  price: number;
  treatmentCategoryId: number;
  categoryName: string;
  imageFile: File;
  isEnabled: boolean;
}


/**
 * Command for PUT /Products/{id}
 * Corresponds to: UpdateProductCommand.cs
 */
export interface UpdateTreatmentsCommand {
  id: number;
  serviceName: string;
  description: string;
  price: number;
  treatmentCategoryId: number;
  categoryName: string;
  imageFile: File | null;
  isEnabled: boolean;
}
