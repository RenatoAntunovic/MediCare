import {Component, inject, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from '@angular/router';
import {forkJoin} from 'rxjs';
import {TreatmentsFormService} from '../services/treatments-form.service';
import {BaseFormComponent} from '../../../../../core/components/base-classes/base-form-component';
import {GetTreatmentsByIdQueryDto, UpdateTreatmentsCommand} from '../../../../../api-services/treatments/treatments-api.models';
import {TreatmentsApiService} from '../../../../../api-services/treatments/treatments-api.service';
import {TreatmentCategoriesApiService} from '../../../../../api-services/treatment-categories/treatment-categories-api.service';
import {ToasterService} from '../../../../../core/services/toaster.service';
import {ListTreatmentCategoriesQueryDto} from '../../../../../api-services/treatment-categories/treatment-categories-api.model';
import {largePaging} from '../../../../../core/models/paging/paging-utils';


@Component({
  selector: 'app-treatments-edit',
  standalone: false,
  templateUrl: './treatments-edit.component.html',
  styleUrl: './treatments-edit.component.scss',
  providers: [TreatmentsFormService]
})
export class TreatmentsEditComponent
  extends BaseFormComponent<GetTreatmentsByIdQueryDto>
  implements OnInit {

  private api = inject(TreatmentsApiService);
  private categoriesApi = inject(TreatmentCategoriesApiService);
  private formService = inject(TreatmentsFormService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private toaster = inject(ToasterService);

  treatmentId!: number;
  categories: ListTreatmentCategoriesQueryDto[] = [];

  ngOnInit(): void {
    this.treatmentId = +this.route.snapshot.params['id'];
    this.initForm(true); // Edit mode
  }

    selectedFile?: File;
    originalImageFile:File | null=null;

    onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
    }
  }

get currentImageSrc(): string | null {
  if (this.model && this.model.imagePath) {
    return `https://localhost:7260/${this.model.imagePath}`;
  }
  return null;
}

  protected loadData(): void {
    this.startLoading();

    // Load product and categories in parallel
    forkJoin({
      treatments: this.api.getById(this.treatmentId),
      categories: this.categoriesApi.list({ onlyEnabled: true, paging: largePaging })
    }).subscribe({
      next: ({ treatments, categories }) => {
        this.model = treatments;
        this.categories = categories.items;
       
        this.form = this.formService.createTreatmentsForm(treatments);
        this.originalImageFile = this.model?.imagePath ? new File([], this.model.imagePath) : null;

        this.stopLoading();
      },
      error: (err) => {
        this.stopLoading('Failed to load treatments');
        this.toaster.error('Treatment not found');
        console.error('Load treatment error:', err);
        this.router.navigate(['/admin/treatments']);
      }
    });
  }

protected save(): void {
  if (this.form.invalid || this.isLoading) return;

  this.startLoading();

  const formValue = this.form.value;
  const formData = new FormData();


formData.append('ServiceName', formValue.name ?? this.model?.serviceName ?? '');
formData.append('Price', (formValue.price ?? this.model?.price ?? 0).toString());
formData.append('Description', formValue.description ?? this.model?.description ?? '');
formData.append('TreatmentCategoryId',(formValue.categoryId ?? this.model?.treatmentsCategoryId ?? 0).toString());
formData.append('isEnabled','true');


if (this.selectedFile) {
  formData.append('ImageFile', this.selectedFile);
} else if (this.originalImageFile) {
  // šalje originalnu sliku ako korisnik nije promijenio
  formData.append('ImageFile', this.originalImageFile);
} else {
  // ako backend dopušta, pošalji prazno polje
  formData.append('ImageFile', '');
}

  this.api.updateFormData(this.treatmentId, formData).subscribe({
    next: () => {
      this.stopLoading();
      this.toaster.success('Treatment updated successfully');
      this.router.navigate(['/admin/treatments']);
    },
    error: (err) => {
      this.stopLoading();
      this.toaster.error('Failed to update treatment');
      console.error('Update treatment error:', err);
    }
  });
}



  onCancel(): void {
    this.router.navigate(['/admin/treatments']);
  }

  getErrorMessage(controlName: string): string {
    return this.formService.getErrorMessage(this.form, controlName);
  }
}
