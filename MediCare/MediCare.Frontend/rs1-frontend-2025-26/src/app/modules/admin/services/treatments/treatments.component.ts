// products.component.ts

import { Component, inject, OnInit,ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import {
  ListTreatmentsRequest,
  ListTreatmentsQueryDto
} from '../../../../api-services/treatments/treatments-api.models';
import {TreatmentsApiService } from '../../../../api-services/treatments/treatments-api.service';
import { BaseListPagedComponent } from '../../../../core/components/base-classes/base-list-paged-component';
import { ToasterService } from '../../../../core/services/toaster.service';
import { DialogHelperService } from '../../../shared/services/dialog-helper.service';
import { DialogButton } from '../../../shared/models/dialog-config.model';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';

@Component({
  selector: 'app-treatment',
  standalone: false,
  templateUrl: './treatments.component.html',
  styleUrl: './treatments.component.scss'
})
export class TreatmentComponent
  extends BaseListPagedComponent<ListTreatmentsQueryDto, ListTreatmentsRequest>
  implements OnInit {

  private api = inject(TreatmentsApiService);
  private router = inject(Router);
  private toaster = inject(ToasterService);
  private dialogHelper = inject(DialogHelperService);

  displayedColumns: string[] = [
    'imageFile',
    'serviceName',
    'treatmentsCategoryName',
    'price',
    'isEnabled',
    'actions'
  ];
  dataSource = new MatTableDataSource<any>([]);

  @ViewChild(MatSort) sort!:MatSort;

  constructor() {
    super();
    this.request = new ListTreatmentsRequest();
    console.log('ADMIN TREATMENT COMPONENT');
  }

    ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    } 

  ngOnInit(): void {
    this.initList();
  }

  protected loadPagedData(): void {
    this.startLoading();

console.log(this.items);

this.api.list(this.request).subscribe({
  next: (response) => {
    console.log('Treatments:', response.items);
    this.items = response.items;
    this.dataSource.data = this.items;
    this.stopLoading();
  },
  error: (err) => {
    console.error('Load error:', err);
    this.stopLoading('Failed to load treatments');
  }
});
  }

  // === UI Actions ===

  onCreate(): void {
    this.router.navigate(['/admin/treatments/add']);
  }

  onEdit(treatment: ListTreatmentsQueryDto): void {
    this.router.navigate(['/admin/treatments', treatment.id, 'edit']);
  }

  onDelete(treatment: ListTreatmentsQueryDto): void {
    this.dialogHelper.treatment.confirmDelete(treatment.serviceName).subscribe(result => {
      if (result && result.button === DialogButton.DELETE) {
        this.performDelete(treatment);
      }
    });
  }

  // === UI Actions ===

onToggleStatus(treatment: ListTreatmentsQueryDto): void {
  if (this.isLoading) return;

  this.startLoading();

  const request$ = treatment.isEnabled
    ? this.api.disable(treatment.id)
    : this.api.enable(treatment.id);

  request$.subscribe({
    next: () => {
      treatment.isEnabled = !treatment.isEnabled;
      this.stopLoading();
    },
    error: (err) => {
      console.error('Toggle status error:', err);
      this.stopLoading();
      this.toaster.error('Failed to update status'); // ili dialog ako želiš
    }
  });
}


  private performDelete(treatment: ListTreatmentsQueryDto): void {
    this.startLoading();

    this.api.delete(treatment.id).subscribe({
      next: () => {
        this.dialogHelper.treatment.showDeleteSuccess().subscribe();
        this.loadPagedData();
      },
      error: (err) => {
        this.stopLoading();

        this.dialogHelper.showError(
          'DIALOGS.TITLES.ERROR',
          'TREATMENTS.DIALOGS.ERROR_DELETE'
        ).subscribe();

        console.error('Delete treatment error:', err);
      }
    });
  }

  onSearch(): void {
    this.request.paging.page = 1;
    this.loadPagedData();
  }
}
