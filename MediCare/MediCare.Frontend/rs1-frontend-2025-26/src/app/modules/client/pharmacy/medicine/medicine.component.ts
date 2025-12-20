// products.component.ts

import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import {
  ListMedicineRequest,
  ListMedicineQueryDto
} from '../../../../api-services/medicine/medicine-api.models';
import { MedicineApiService } from '../../../../api-services/medicine/medicine-api.service';
import { BaseListPagedComponent } from '../../../../core/components/base-classes/base-list-paged-component';
import { ToasterService } from '../../../../core/services/toaster.service';
import { DialogHelperService } from '../../../shared/services/dialog-helper.service';
import { DialogButton } from '../../../shared/models/dialog-config.model';
import { ListMedicineCategoriesQueryDto, ListMedicineCategoriesRequest } from '../../../../api-services/medicine-categories/medicine-categories-api.model';
import { MedicineCategoriesApiService } from '../../../../api-services/medicine-categories/medicine-categories-api.service';

@Component({
  selector: 'app-medicine',
  standalone: false,
  templateUrl: './medicine.component.html',
  styleUrl: './medicine.component.scss'
})
export class MedicineComponent
  extends BaseListPagedComponent<ListMedicineQueryDto, ListMedicineRequest>
  implements OnInit {

  private api = inject(MedicineApiService);
  private categoryApi = inject(MedicineCategoriesApiService);
  private router = inject(Router);
  private toaster = inject(ToasterService);
  private dialogHelper = inject(DialogHelperService);

  displayedColumns: string[] = [
    'imageFile',
    'name',
    'medicineCategoryName',
    'price',
    'weight',
    'isEnabled',
    'actions'
  ];

categories: ListMedicineCategoriesQueryDto[] = [];
categoryFilter: number | null = null;

  constructor() {
    super();
    this.request = new ListMedicineRequest();
  }

  ngOnInit(): void {
    this.initList();
    this.loadCategories();
  }

  onCategoryFilterChange(categoryId: number | null): void {
  this.categoryFilter = categoryId;        // čuva selektovanu kategoriju
  this.request.categoryId = categoryId;    // postavlja filter u request
  this.request.paging.page = 1;            // resetuje paging
  this.loadPagedData();                    // reload medicine liste
}

private loadCategories():void{
  const request = new ListMedicineCategoriesRequest();
  request.onlyEnabled = true;
  request.paging.page = 1;
  request.paging.pageSize = 100; // dovoljno za dropdown

  this.categoryApi.list(request).subscribe(res => {
    this.categories = res.items;
  });
}

onCreate() {}
onEdit(medicine: any) {}
onDelete(medicine: any) {}
onToggleStatus(medicine: any) {}

  protected loadPagedData(): void {
    this.startLoading();

this.api.list(this.request).subscribe({
  next: (response) => {
    console.log('Medicines:', response.items);
    this.items = response.items;
    this.stopLoading();
  },
  error: (err) => {
    console.error('Load error:', err);
    this.stopLoading('Failed to load medicines');
  }
});
  }

  // === UI Actions ===


  onSearch(): void {
    this.request.paging.page = 1;
    this.loadPagedData();
  }
}
