import { PageResult } from '../../core/models/paging/page-result';
import { BasePagedQuery } from '../../core/models/paging/base-paged-query';

// === QUERIES (READ) ===

/**
 * Query parameters for GET /Products
 * Corresponds to: ListProductsQuery.cs
 */
export class ListMedicineRequest extends BasePagedQuery {
  search?: string | null;
  categoryId?:number | null;
  // Future filters: categoryId?, isEnabled?, priceMin?, priceMax?
}

/**
 * Response item for GET /Products
 * Corresponds to: ListProductsQueryDto.cs
 */
export interface ListMedicineQueryDto {
  id: number;
  name: string;
  description: string;
  price: number;
  medicineCategoryId: number;
  categoryName: string;
  imagePath: string;
  weight: number;
  isEnabled: boolean;
}


/**
 * Response for GET /Products/{id}
 * Corresponds to: GetProductByIdQueryDto.cs
 */
export interface GetMedicineByIdQueryDto {
  id: number;
  name: string;
  description: string;
  price: number;
  medicineCategoryId: number;
  categoryName: string;
  imagePath: string;
  weight: number;
  isEnabled: boolean;
}

export interface GetMedicineByCategoryIdQueryDto {
  id: number;
  name: string;
  description: string;
  price: number;
  medicineCategoryId: number;
  categoryName: string;
  imagePath: string;
  weight: number;
  isEnabled: boolean;
}

/**
 * Paged response for GET /Products
 */
export type ListMedicineResponse = PageResult<ListMedicineQueryDto>;

// === COMMANDS (WRITE) ===

/**
 * Command for POST /Products
 * Corresponds to: CreateProductCommand.cs
 */
export interface CreateMedicineCommand {
  name: string;
  description: string;
  price: number;
  categoryId: number;
  ImageFile?: File; // IFormFile se mapira na File u JS/TS
  weight: number;
  isEnabled: boolean;
}


/**
 * Command for PUT /Products/{id}
 * Corresponds to: UpdateProductCommand.cs
 */
export interface UpdateMedicineCommand {
  id: number;
  name: string;
  description: string;
  price: number;
  categoryId: number;
  weight: number;
  ImageFile?:File | null;
}
