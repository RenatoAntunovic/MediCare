import { TestBed } from '@angular/core/testing';
import { TranslateService } from '@ngx-translate/core';
import { of, Subject } from 'rxjs';
import { AdminStatisticsComponent } from './admin-statistics.component';
import { StatisticsApiService } from '../../../api-services/statistics/statistics-api.service';
import { StatisticsDashboardDto } from '../../../api-services/statistics/statistics-api.models';

describe('AdminStatisticsComponent (locale + revenue)', () => {
  const dashboard: StatisticsDashboardDto = {
    from: null, to: null,
    ordersRevenue: 1234.5, reservationsRevenue: 100,
    ordersConfirmedAmount: 30, reservationsConfirmedAmount: 20,
    ordersCount: 3, reservationsCount: 2, newUsersCount: 0, lowStockCount: 0,
    byMonth: [{ year: 2026, month: 3, ordersRevenue: 1234.5, reservationsRevenue: 100, ordersCount: 3, reservationsCount: 2 }],
    ordersByStatus: [], reservationsByStatus: [], reservationsByTreatment: [],
    reservationsByWeekday: [], topMedicines: [], lowStock: []
  };

  let langChange: Subject<{ lang: string }>;
  let component: AdminStatisticsComponent;

  beforeEach(() => {
    langChange = new Subject();
    const translate = { currentLang: 'bs', onLangChange: langChange, instant: (key: string) => key };

    TestBed.configureTestingModule({
      declarations: [AdminStatisticsComponent],
      providers: [
        { provide: StatisticsApiService, useValue: { dashboard: vi.fn().mockReturnValue(of(dashboard)) } },
        { provide: TranslateService, useValue: translate }
      ]
    }).overrideComponent(AdminStatisticsComponent, { set: { template: '' } });

    component = TestBed.createComponent(AdminStatisticsComponent).componentInstance;
    component.ngOnInit();
  });

  it('formats the same amount in Bosnian and in English (key figure cards)', () => {
    expect(component.formatMoney(1234.5)).toBe('KM 1.234,50');   // selected language: bs

    langChange.next({ lang: 'en' });
    expect(component.formatMoney(1234.5)).toBe('KM 1,234.50');

    langChange.next({ lang: 'bs' });
    expect(component.formatMoney(1234.5)).toBe('KM 1.234,50');
  });

  it('month labels of the charts follow the selected language too', () => {
    const bs = component.revenueChart().labels[0];
    langChange.next({ lang: 'en' });
    const en = component.revenueChart().labels[0];

    expect(en).toBe('Mar 2026');
    expect(bs).not.toBe(en);
  });

  it('shows the confirmed amount apart, the total revenue is only the paid part', () => {
    expect(component.totalRevenue()).toBe(1334.5);     // paid orders + paid reservations
    expect(component.confirmedNotPaid()).toBe(50);     // confirmed orders + reservations
  });
});
