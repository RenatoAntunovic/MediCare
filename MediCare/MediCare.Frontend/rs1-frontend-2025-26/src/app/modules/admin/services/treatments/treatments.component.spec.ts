import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { TreatmentComponent } from './treatments.component';
import { TreatmentsApiService } from '../../../../api-services/treatments/treatments-api.service';
import { ToasterService } from '../../../../core/services/toaster.service';
import { DialogHelperService } from '../../../shared/services/dialog-helper.service';

describe('Admin TreatmentComponent (paging)', () => {
  // the backend answers with "total" = size of the FILTERED set, "items" = one page of it
  const page = (count: number, total: number) => ({
    items: Array.from({ length: count }, (_, i) => ({ id: i + 1, serviceName: `T${i + 1}` })),
    total
  });

  let api: { list: ReturnType<typeof vi.fn> };
  let component: TreatmentComponent;

  beforeEach(() => {
    api = { list: vi.fn().mockReturnValue(of(page(10, 25))) };

    TestBed.configureTestingModule({
      declarations: [TreatmentComponent],
      providers: [
        { provide: TreatmentsApiService, useValue: api },
        { provide: Router, useValue: { navigate: vi.fn() } },
        { provide: ToasterService, useValue: {} },
        { provide: DialogHelperService, useValue: {} }
      ]
    }).overrideComponent(TreatmentComponent, { set: { template: '' } }); // class logic only

    component = TestBed.createComponent(TreatmentComponent).componentInstance;
    component.ngOnInit();
  });

  it('uses the total from the server response for the paginator', () => {
    expect(component.items.length).toBe(10);
    expect(component.totalItems).toBe(25);          // not 10 (items on this page)
    expect(component.totalPages).toBe(3);           // ceil(25 / 10)
  });

  it('goes back to page 1 when a search reduces the number of pages', () => {
    component.goToPage(2);
    expect(api.list).toHaveBeenLastCalledWith(expect.objectContaining({ paging: expect.objectContaining({ page: 2 }) }));

    api.list.mockReturnValue(of(page(3, 3)));       // the filter leaves only 3 treatments
    component.request.search = 'massage';
    component.onSearch();

    expect(component.paging.page).toBe(1);
    expect(component.totalItems).toBe(3);
    expect(component.totalPages).toBe(1);
  });

  it('goes back to page 1 when the sort changes', () => {
    component.goToPage(3);
    component.onSortChange({ active: 'price', direction: 'desc' });

    expect(component.paging.page).toBe(1);
    expect(component.request.sortBy).toBe('price');
  });
});
