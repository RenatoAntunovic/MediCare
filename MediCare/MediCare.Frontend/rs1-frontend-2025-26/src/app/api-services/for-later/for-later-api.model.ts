import { BasePagedQuery } from '../../core/models/paging/base-paged-query';
import { PageResult } from '../../core/models/paging/page-result';

export interface ForLaterDto {
  id:number;
  medicineId:number;
  medicineName:string;
  price:number;
  imagePath:string;
}

export interface AddToForLaterCommand {
  medicineId: number;
}

export interface DeleteForLaterCommand {
  id: number; 
}