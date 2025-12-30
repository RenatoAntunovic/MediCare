// products.component.ts

import { Component, inject, OnInit } from '@angular/core';
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

@Component({
  selector: 'app-treatments',
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
    'name',
    'treatmentsCategoryName',
    'price',
    'isEnabled',
    'actions'
  ];

  private lastRequestTime = 0;
  private requestCooldown = 2000;

  constructor() {
    super();
    this.request = new ListTreatmentsRequest();
      console.log('CLIENT TREATMENT COMPONENT');
  }

  ngOnInit(): void {
    this.initList();
  }

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
      console.log('Treatments:', response.items);
      this.items = response.items;
      this.stopLoading();
    },
    error: (err) => {
      console.error('Load error:', err);
      this.stopLoading('Failed to load treatments');
    }
  });
}

  // === UI Actions ===

  
  onCreate() {}
  onEdit(medicine: any) {}
  onDelete(medicine: any) {}
  onToggleStatus(medicine: any) {}

  onSearch(): void {
    this.request.paging.page = 1;
    this.loadPagedData();
  }
}
