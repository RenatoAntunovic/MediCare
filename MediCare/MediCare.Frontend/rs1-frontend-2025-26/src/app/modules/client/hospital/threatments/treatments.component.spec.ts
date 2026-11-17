import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';
import { TreatmentComponent } from './treatments.component';
import { TreatmentsApiService } from '../../../../api-services/treatments/treatments-api.service';
import { ToasterService } from '../../../../core/services/toaster.service';

describe('Client TreatmentComponent (paging)', () => {
  const page = (count: number, total: number) => ({
    items: Array.from({ length: count }, (_, i) => ({ id: i + 1, serviceName: `T${i + 1}` })),
    total
  });

  let api: { list: ReturnType<typeof vi.fn> };
  let component: TreatmentComponent;

  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-01-01T10:00:00'));
    api = { list: vi.fn().mockReturnValue(of(page(10, 25))) };

    TestBed.configureTestingModule({
      declarations: [TreatmentComponent],
      providers: [
        { provide: TreatmentsApiService, useValue: api },
        { provide: ToasterService, useValue: { error: vi.fn() } },
        { provide: MatDialog, useValue: {} }
      ]
    }).overrideComponent(TreatmentComponent, { set: { template: '' } });

    component = TestBed.createComponent(TreatmentComponent).componentInstance;
    component.ngOnInit();
  });

  afterEach(() => vi.useRealTimers());

  it('uses the total from the server response for the paginator', () => {
    expect(component.items.length).toBe(10);
    expect(component.totalItems).toBe(25);
    expect(component.totalPages).toBe(3);
  });

  it('goes back to page 1 when a search reduces the number of pages', () => {
    component.goToPage(2);                          // (cooldown of the client list is 2 s)
    vi.advanceTimersByTime(3000);

    api.list.mockReturnValue(of(page(3, 3)));
    component.request.search = 'massage';
    component.onSearch();

    expect(component.paging.page).toBe(1);
    expect(component.totalItems).toBe(3);
    expect(component.totalPages).toBe(1);
  });
});
