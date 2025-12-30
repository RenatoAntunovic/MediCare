import { PageResult } from '../../core/models/paging/page-result';
import { BasePagedQuery } from '../../core/models/paging/base-paged-query';

// === QUERIES (READ) ===

/**
 * Query parameters for GET /Medicine
 * Corresponds to: ListMedicineQuery.cs
 */
export class ListMedicineRequest extends BasePagedQuery {
  search?: string | null;
  categoryId?:number | null;
}

/**
 * Response item for GET /Medicine
 * Corresponds to: ListMedicineQueryDto.cs
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
 * Response for GET /Medicine/{id}
 * Corresponds to: GetMedicineByIdQueryDto.cs
 */
export interface GetMedicineByIdQueryDto {
  id: number;
  name: string;
  description: string;
  price: number;
  medicineCategoryId: number;
  medicineCategoryName: string;
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
 * Paged response for GET /Medicine
 */
export type ListMedicineResponse = PageResult<ListMedicineQueryDto>;

// === COMMANDS (WRITE) ===

/**
 * Command for POST /Medicine
 * Corresponds to: CreateMedicineCommand.cs
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
 * Command for PUT /Medicine/{id}
 * Corresponds to: UpdateMedicineCommand.cs
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
