import { BasePagedQuery } from '../../core/models/paging/base-paged-query';
import { PageResult } from '../../core/models/paging/page-result';

export interface FavouritesDto {
  id:number;
  medicineId:number;
  medicineName:string;
  price:number;
  imagePath:string;
}

export interface AddToFavouritesCommand {
  medicineId: number;
}

export interface DeleteFavouritesCommand {
  id: number; 
}