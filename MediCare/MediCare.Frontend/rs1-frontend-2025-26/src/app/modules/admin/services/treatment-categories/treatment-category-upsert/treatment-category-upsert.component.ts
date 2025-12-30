import { Component, inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormGroup } from '@angular/forms';
import { TreatmentCategoriesApiService } from '../../../../../api-services/treatment-categories/treatment-categories-api.service';
import { ToasterService } from '../../../../../core/services/toaster.service';
import {
  GetTreatmentCategoryByIdQueryDto,
  CreateTreatmentCategoryCommand,
  UpdateTreatmentCategoryCommand
} from '../../../../../api-services/treatment-categories/treatment-categories-api.model';
import {TreatmentCategoryFormService} from "../services/treatment-category-form.service";

export interface TreatmentCategoryDialogData {
  mode: 'create' | 'edit';
  categoryId?: number;
}

@Component({
  selector: 'app-treatment-category-upsert',
  standalone: false,
  templateUrl: './treatment-category-upsert.component.html',
  styleUrl: './treatment-category-upsert.component.scss',
  providers: [TreatmentCategoryFormService]
})
export class TreatmentCategoryUpsertComponent implements OnInit {
  private dialogRef = inject(MatDialogRef<TreatmentCategoryUpsertComponent>);
  private data = inject<TreatmentCategoryDialogData>(MAT_DIALOG_DATA);
  private api = inject(TreatmentCategoriesApiService);
  private formService = inject(TreatmentCategoryFormService);
  private toaster = inject(ToasterService);

  form!: FormGroup;
  isLoading = false;
  isEditMode = false;
  title = '';

  ngOnInit(): void {
    this.isEditMode = this.data.mode === 'edit';
    this.title = this.isEditMode ? 'Edit Category' : 'New Category';

    if (this.isEditMode && this.data.categoryId) {
      this.loadCategory(this.data.categoryId);
    } else {
      this.form = this.formService.createCategoryForm();
    }
  }

private loadCategory(id: number): void {
  this.isLoading = true;

  this.api.getById(id).subscribe({
    next: (category) => {
      // Mapiramo name → categoryName za formu
      const mappedCategory = {
        id: category.id,
        isEnabled: category.isEnabled,
        categoryName: (category as any).name ?? category.categoryName
      };

      // Popunjavamo formu
      this.form = this.formService.createCategoryForm(mappedCategory);

      this.isLoading = false;
    },
    error: (err) => {
      this.toaster.error('Failed to load category');
      console.error('Load category error:', err);
      this.dialogRef.close(false);
    }
  });
}


  onSubmit(): void {
    if (this.form.invalid || this.isLoading) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading = true;

    if (this.isEditMode && this.data.categoryId) {
      this.updateCategory();
    } else {
      this.createCategory();
    }
  }

  private createCategory(): void {
    const command: CreateTreatmentCategoryCommand = {
      name: this.form.value.name.trim()
    };

    this.api.create(command).subscribe({
      next: () => {
        this.toaster.success('Category created successfully');
        this.dialogRef.close(true); // Signal success
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Create category error:', err);
      }
    });
  }

  private updateCategory(): void {
    const command: UpdateTreatmentCategoryCommand = {
      name: this.form.value.name.trim()
    };

    this.api.update(this.data.categoryId!, command).subscribe({
      next: () => {
        this.toaster.success('Category updated successfully');
        this.dialogRef.close(true); // Signal success
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Update category error:', err);
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  getErrorMessage(controlName: string): string {
    return this.formService.getErrorMessage(this.form, controlName);
  }
}
