import { BasePagedQuery } from '../../core/models/paging/base-paged-query';
import { PageResult } from '../../core/models/paging/page-result';

/**
 * Query for GET /api/suppliers – 5 filters, all applied on the server before paging.
 * Corresponds to: ListSuppliersQuery.cs
 */
export class ListSuppliersRequest extends BasePagedQuery {
  search?: string | null;
  city?: string | null;
  onlyEnabled?: boolean | null;
  medicineId?: number | null;
  /** ISO date "yyyy-MM-dd" */
  createdFrom?: string | null;
}

/** Corresponds to: ListSuppliersQueryDto.cs */
export interface ListSuppliersQueryDto {
  id: number;
  companyName: string;
  contactName: string;
  email: string;
  phone: string;
  city: string;
  isEnabled: boolean;
  medicinesCount: number;
  createdAtUtc: string;
}

export type ListSuppliersResponse = PageResult<ListSuppliersQueryDto>;

/** Corresponds to: GetSupplierByIdQueryDto.cs */
export interface GetSupplierByIdQueryDto {
  id: number;
  companyName: string;
  contactName: string;
  email: string;
  phone: string;
  address: string;
  city: string;
  isEnabled: boolean;
  medicineIds: number[];
}

/** Body for POST /api/suppliers and PUT /api/suppliers/{id} (SupplierFields.cs) */
export interface UpsertSupplierCommand {
  companyName: string;
  contactName: string;
  email: string;
  phone: string;
  address: string;
  city: string;
  isEnabled: boolean;
  medicineIds: number[];
}
