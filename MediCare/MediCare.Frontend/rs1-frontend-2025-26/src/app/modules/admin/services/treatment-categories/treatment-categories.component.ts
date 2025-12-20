import { Component, inject, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { BaseListPagedComponent } from '../../../../core/components/base-classes/base-list-paged-component';
import { TreatmentCategoriesApiService } from '../../../../api-services/treatment-categories/treatment-categories-api.service';
import { ToasterService } from '../../../../core/services/toaster.service';
import { DialogHelperService } from '../../../shared/services/dialog-helper.service';
import { DialogButton } from '../../../shared/models/dialog-config.model';
import {
  ListTreatmentCategoriesRequest,
  ListTreatmentCategoriesQueryDto,
} from '../../../../api-services/treatment-categories/treatment-categories-api.model';
import { TreatmentCategoryUpsertComponent } from './treatment-category-upsert/treatment-category-upsert.component';

@Component({
  selector: 'app-treatment-categories',
  standalone: false,
  templateUrl: './treatment-categories.component.html',
  styleUrl: './treatment-categories.component.scss',
})
export class TreatmentCategoriesComponent
  extends BaseListPagedComponent<ListTreatmentCategoriesQueryDto, ListTreatmentCategoriesRequest>
  implements OnInit
{
  private api = inject(TreatmentCategoriesApiService);
  private dialog = inject(MatDialog);
  private toaster = inject(ToasterService);
  private dialogHelper = inject(DialogHelperService);

  displayedColumns: string[] = ['name', 'isEnabled', 'actions'];
  showOnlyEnabled = true;

  constructor() {
    super();
    this.request = new ListTreatmentCategoriesRequest();
    this.request.onlyEnabled = true;
  }

  ngOnInit(): void {
    this.initList();
  }

  protected loadPagedData(): void {
    this.startLoading();

    this.api.list(this.request).subscribe({
      next: (response) => {
        // mapiramo categoryName u name
        const itemsWithName = response.items.map(item => ({
          ...item,
          name: item.categoryName
        }));

        this.handlePageResult({ ...response, items: itemsWithName });
        this.stopLoading();
      },
      error: (err) => {
        this.stopLoading('Failed to load categories');
        console.error('Load categories error:', err);
      },
    });
  }

  // === Filters ===
  onSearchChange(searchTerm: string): void {
    this.request.search = searchTerm;
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  onToggleEnabledFilter(checked: boolean): void {
    this.showOnlyEnabled = checked;
    this.request.onlyEnabled = checked;
    this.request.paging.page = 1;
    this.loadPagedData();
  }

  // === CRUD Actions ===
  onCreate(): void {
    const dialogRef = this.dialog.open(TreatmentCategoryUpsertComponent, {
      width: '500px',
      maxWidth: '90vw',
      panelClass: 'treatment-category-dialog',
      autoFocus: true,
      disableClose: false,
      data: { mode: 'create' },
    });

    dialogRef.afterClosed().subscribe((success: boolean) => {
      if (success) {
        this.dialogHelper.treatmentCategory.showCreateSuccess().subscribe();
        this.loadPagedData();
      }
    });
  }

  onEdit(category: ListTreatmentCategoriesQueryDto): void {
    const dialogRef = this.dialog.open(TreatmentCategoryUpsertComponent, {
      width: '500px',
      maxWidth: '90vw',
      panelClass: 'treatment-category-dialog',
      autoFocus: true,
      disableClose: false,
      data: { mode: 'edit', categoryId: category.id },
    });

    dialogRef.afterClosed().subscribe((success: boolean) => {
      if (success) {
        this.dialogHelper.treatmentCategory.showUpdateSuccess().subscribe();
        this.loadPagedData();
      }
    });
  }

  onDelete(category: ListTreatmentCategoriesQueryDto): void {
    this.dialogHelper.treatmentCategory.confirmDelete(category.categoryName).subscribe(result => {
      if (result && result.button === DialogButton.DELETE) {
        this.performDelete(category);
      }
    });
  }

  private performDelete(category: ListTreatmentCategoriesQueryDto): void {
    this.startLoading();
    this.api.delete(category.id).subscribe({
      next: () => {
        this.dialogHelper.treatmentCategory.showDeleteSuccess().subscribe();
        this.loadPagedData();
      },
      error: (err) => {
        this.stopLoading();
        console.error('Delete category error:', err);
        this.dialogHelper.showError(
          'DIALOGS.TITLES.ERROR',
          'PRODUCT_CATEGORIES.DIALOGS.ERROR_DELETE'
        ).subscribe();
      },
    });
  }

  onToggleStatus(category: ListTreatmentCategoriesQueryDto): void {
    this.startLoading();

    const apiAction = category.isEnabled
      ? this.api.disable(category.id)
      : this.api.enable(category.id);

    apiAction.subscribe({
      next: () => {
        const status = category.isEnabled ? 'disabled' : 'enabled';
        this.toaster.success(`Category ${status} successfully`);
        this.loadPagedData();
      },
      error: (err) => {
        this.stopLoading();
        console.error('Toggle status error:', err);
      },
    });
  }
}
