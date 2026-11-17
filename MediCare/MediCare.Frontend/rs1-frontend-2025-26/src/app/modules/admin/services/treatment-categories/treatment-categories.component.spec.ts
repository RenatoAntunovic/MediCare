import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';
import { TreatmentCategoriesComponent } from './treatment-categories.component';
import { TreatmentCategoriesApiService } from '../../../../api-services/treatment-categories/treatment-categories-api.service';
import { ToasterService } from '../../../../core/services/toaster.service';
import { DialogHelperService } from '../../../shared/services/dialog-helper.service';

describe('TreatmentCategoriesComponent (paging + filters)', () => {
  const page = (count: number, total: number) => ({
    items: Array.from({ length: count }, (_, i) => ({ id: i + 1, categoryName: `C${i + 1}`, isEnabled: true })),
    total
  });

  let api: { list: ReturnType<typeof vi.fn> };
  let component: TreatmentCategoriesComponent;

  beforeEach(() => {
    api = { list: vi.fn().mockReturnValue(of(page(10, 25))) };

    TestBed.configureTestingModule({
      declarations: [TreatmentCategoriesComponent],
      providers: [
        { provide: TreatmentCategoriesApiService, useValue: api },
        { provide: MatDialog, useValue: {} },
        { provide: ToasterService, useValue: {} },
        { provide: DialogHelperService, useValue: {} }
      ]
    }).overrideComponent(TreatmentCategoriesComponent, { set: { template: '' } });

    component = TestBed.createComponent(TreatmentCategoriesComponent).componentInstance;
    component.ngOnInit();
  });

  it('uses the total from the server response for the paginator', () => {
    expect(component.totalItems).toBe(25);
    expect(component.totalPages).toBe(3);
  });

  it('goes back to page 1 when the search changes', () => {
    component.goToPage(2);
    api.list.mockReturnValue(of(page(2, 2)));
    component.onSearchChange('den');

    expect(component.paging.page).toBe(1);
    expect(component.totalItems).toBe(2);
  });

  it('switching "only enabled" off asks for ALL categories (null), not only the disabled ones (false)', () => {
    component.goToPage(2);
    component.onToggleEnabledFilter(false);

    expect(component.request.onlyEnabled).toBeNull();
    expect(component.paging.page).toBe(1);

    component.onToggleEnabledFilter(true);
    expect(component.request.onlyEnabled).toBe(true);
  });
});
