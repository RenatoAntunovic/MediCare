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
import { FavouritesService } from '../../../../api-services/favourites/favourites-api.service';
import { MatSnackBar } from '@angular/material/snack-bar';

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
  private favoritesService = inject(FavouritesService);
  private snackbar = inject(MatSnackBar);
  isDarkMode=false;

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

private lastRequestTime = 0;
private requestCooldown = 2000;

  constructor() {
    super();
    this.request = new ListMedicineRequest();
  }


  
  toggleDarkMode(): void {
    this.isDarkMode = !this.isDarkMode;
    document.body.classList.toggle('dark-mode', this.isDarkMode);
    localStorage.setItem('darkMode', this.isDarkMode ? 'true' : 'false');
  }

goToMedicineDetail(medicine: any) {
  this.router.navigate(['/client/medicine', medicine.id]);
}



  ngOnInit(): void {
    this.initList();
    this.loadCategories();
    const darkMode = localStorage.getItem('darkMode');
  if (darkMode === 'true') {
    document.body.classList.add('dark-mode');
  }
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

  addToFavourites(item: any) {
    const command = { medicineId: item.id }; // AddToFavouritesCommand payload

    this.favoritesService.addToFavourites(command).subscribe({
      next: (res: { favouriteId: number }) => {
        this.snackbar.open('Dodano u favorite', '', {
          duration: 2000,
          panelClass: ['custom-snackbar']
        });

        // opciono: ažurirati item ili dataSource da pokaže da je već u favorite
        item.isFavourite = true;
      },
      error: () => {
        this.snackbar.open('Greška prilikom dodavanja u favorite', '', {
          duration: 2000,
          panelClass: ['custom-snackbar']
        });
      }
    });
  }


onCreate() {}
onEdit(medicine: any) {}
onDelete(medicine: any) {}
onToggleStatus(medicine: any) {}

protected loadPagedData(): void {
  const now = Date.now();
  if (now - this.lastRequestTime < this.requestCooldown) {
    this.toaster.error('Too many requests, please wait a few seconds.');
    return;
  }
  this.lastRequestTime = now;

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
