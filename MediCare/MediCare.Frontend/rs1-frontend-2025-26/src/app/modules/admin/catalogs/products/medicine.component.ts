// products.component.ts

import { Component, inject, OnInit, ViewChild } from '@angular/core';
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
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';

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
  dataSource = new MatTableDataSource<any>([]);

  @ViewChild(MatSort) sort!:MatSort;

  private lastRequestTime = 0;
  private requestCooldown = 2000;

  constructor() {
    super();
    this.request = new ListMedicineRequest();
  }

    ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
  }

  ngOnInit(): void {
    this.initList();
  }

  protected loadPagedData(): void {
    this.startLoading();

this.api.list(this.request).subscribe({
  next: (response) => {
    console.log('Medicines:', response.items);
    this.items = response.items;
    this.dataSource.data = this.items;
    this.stopLoading();
  },
  error: (err) => {
    console.error('Load error:', err);
    this.stopLoading('Failed to load medicines');
  }
});
  }

  // === UI Actions ===

  onCreate(): void {
    this.router.navigate(['/admin/products/add']);
  }

  onEdit(medicine: ListMedicineQueryDto): void {
    this.router.navigate(['/admin/products', medicine.id, 'edit']);
  }

  onDelete(medicine: ListMedicineQueryDto): void {
    this.dialogHelper.medicine.confirmDelete(medicine.name).subscribe(result => {
      if (result && result.button === DialogButton.DELETE) {
        this.performDelete(medicine);
      }
    });
  }

  onToggleStatus(medicine: ListMedicineQueryDto): void {
    if (this.isLoading) return;
  
    this.startLoading();
  
    const request$ = medicine.isEnabled
      ? this.api.disable(medicine.id)
      : this.api.enable(medicine.id);
  
    request$.subscribe({
      next: () => {
        medicine.isEnabled = !medicine.isEnabled;
        this.stopLoading();
      },
      error: (err) => {
        console.error('Toggle status error:', err);
        this.stopLoading();
        this.toaster.error('Failed to update status'); // ili dialog ako želiš
      }
    });
  }

  private performDelete(medicine: ListMedicineQueryDto): void {
    this.startLoading();

    this.api.delete(medicine.id).subscribe({
      next: () => {
        this.dialogHelper.medicine.showDeleteSuccess().subscribe();
        this.loadPagedData();
      },
      error: (err) => {
        this.stopLoading();

        this.dialogHelper.showError(
          'DIALOGS.TITLES.ERROR',
          'MEDICINE.DIALOGS.ERROR_DELETE'
        ).subscribe();

        console.error('Delete medicine error:', err);
      }
    });
  }

  onSearch(): void {
    this.request.paging.page = 1;
    this.loadPagedData();
  }
}
