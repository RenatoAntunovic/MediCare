import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TranslateService } from '@ngx-translate/core';
import { StatisticsApiService } from '../../../api-services/statistics/statistics-api.service';
import { StatisticsDashboardDto } from '../../../api-services/statistics/statistics-api.models';
import { McChartSeries } from '../../shared/components/mc-chart/mc-chart.component';

type Period = 'all' | '30d' | '3m' | '12m' | 'year';

interface ChartData {
  labels: string[];
  series: McChartSeries[];
}

/**
 * Admin statistics: summary tiles + Chart.js charts for orders, reservations and stock.
 * All numbers come from one backend call (GET /api/Statistics/dashboard).
 */
@Component({
  selector: 'app-admin-statistics',
  standalone: false,
  templateUrl: './admin-statistics.component.html',
  styleUrl: './admin-statistics.component.scss',
})
export class AdminStatisticsComponent implements OnInit {
  private api = inject(StatisticsApiService);
  private translate = inject(TranslateService);
  private destroyRef = inject(DestroyRef);

  readonly periods: { value: Period; label: string }[] = [
    { value: 'all', label: 'STATISTICS.PERIOD.ALL' },
    { value: '30d', label: 'STATISTICS.PERIOD.LAST_30_DAYS' },
    { value: '3m', label: 'STATISTICS.PERIOD.LAST_3_MONTHS' },
    { value: '12m', label: 'STATISTICS.PERIOD.LAST_12_MONTHS' },
    { value: 'year', label: 'STATISTICS.PERIOD.THIS_YEAR' },
  ];

  period = signal<Period>('all');
  data = signal<StatisticsDashboardDto | null>(null);
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);

  /** Bumped on language change so translated chart labels are rebuilt */
  private lang = signal(this.translate.currentLang || 'bs');

  totalRevenue = computed(() => {
    const d = this.data();
    return d ? d.ordersRevenue + d.reservationsRevenue : 0;
  });

  revenueChart = computed<ChartData>(() => {
    const d = this.data();
    this.lang();
    if (!d) return { labels: [], series: [] };

    return {
      labels: d.byMonth.map((m) => this.monthLabel(m.year, m.month)),
      series: [
        { label: this.translate.instant('STATISTICS.SERIES.MEDICINES'), data: d.byMonth.map((m) => m.ordersRevenue), slot: 1 },
        { label: this.translate.instant('STATISTICS.SERIES.TREATMENTS'), data: d.byMonth.map((m) => m.reservationsRevenue), slot: 2 },
      ],
    };
  });

  activityChart = computed<ChartData>(() => {
    const d = this.data();
    this.lang();
    if (!d) return { labels: [], series: [] };

    return {
      labels: d.byMonth.map((m) => this.monthLabel(m.year, m.month)),
      series: [
        { label: this.translate.instant('STATISTICS.SERIES.ORDERS'), data: d.byMonth.map((m) => m.ordersCount), slot: 1 },
        { label: this.translate.instant('STATISTICS.SERIES.RESERVATIONS'), data: d.byMonth.map((m) => m.reservationsCount), slot: 2 },
      ],
    };
  });

  ordersByStatusChart = computed<ChartData>(() => {
    const d = this.data();
    this.lang();
    if (!d) return { labels: [], series: [] };

    return {
      labels: d.ordersByStatus.map((s) => this.statusLabel(s.label)),
      series: [{ label: this.translate.instant('STATISTICS.SERIES.ORDERS'), data: d.ordersByStatus.map((s) => s.count), slot: 1 }],
    };
  });

  topMedicinesChart = computed<ChartData>(() => {
    const d = this.data();
    this.lang();
    if (!d) return { labels: [], series: [] };

    return {
      labels: d.topMedicines.map((m) => m.name),
      series: [{ label: this.translate.instant('STATISTICS.SERIES.SOLD_QUANTITY'), data: d.topMedicines.map((m) => m.quantity), slot: 1 }],
    };
  });

  reservationsByTreatmentChart = computed<ChartData>(() => {
    const d = this.data();
    this.lang();
    if (!d) return { labels: [], series: [] };

    return {
      labels: d.reservationsByTreatment.map((t) => t.label),
      series: [{ label: this.translate.instant('STATISTICS.SERIES.RESERVATIONS'), data: d.reservationsByTreatment.map((t) => t.count), slot: 2 }],
    };
  });

  reservationsByWeekdayChart = computed<ChartData>(() => {
    const d = this.data();
    this.lang();
    if (!d) return { labels: [], series: [] };

    return {
      labels: d.reservationsByWeekday.map((w) => this.translate.instant(`STATISTICS.WEEKDAYS.${w.label.toUpperCase()}`)),
      series: [{ label: this.translate.instant('STATISTICS.SERIES.RESERVATIONS'), data: d.reservationsByWeekday.map((w) => w.count), slot: 2 }],
    };
  });

  hasMonthlyData = computed(() => (this.data()?.byMonth.length ?? 0) > 0);
  hasReservations = computed(() => (this.data()?.reservationsCount ?? 0) > 0);

  ngOnInit(): void {
    this.translate.onLangChange
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((e) => this.lang.set(e.lang));

    this.load();
  }

  onPeriodChange(value: Period): void {
    this.period.set(value);
    this.load();
  }

  load(): void {
    const { from, to } = this.periodRange(this.period());
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.api
      .dashboard({ from, to })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.data.set(result);
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set(this.translate.instant('STATISTICS.LOAD_ERROR'));
          this.isLoading.set(false);
        },
      });
  }

  formatMoney(value: number): string {
    return 'KM ' + value.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  private periodRange(period: Period): { from: string | null; to: string | null } {
    const today = new Date();
    const start = new Date(today);

    switch (period) {
      case 'all':
        return { from: null, to: null };
      case '30d':
        start.setDate(today.getDate() - 29);
        break;
      case '3m':
        start.setMonth(today.getMonth() - 2, 1);
        break;
      case '12m':
        start.setMonth(today.getMonth() - 11, 1);
        break;
      case 'year':
        start.setMonth(0, 1);
        break;
    }

    return { from: this.toIsoDate(start), to: this.toIsoDate(today) };
  }

  /** Local date as yyyy-MM-dd (toISOString would shift it to UTC) */
  private toIsoDate(d: Date): string {
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
  }

  private monthLabel(year: number, month: number): string {
    const locale = this.lang() === 'en' ? 'en-GB' : 'bs-BA';
    return new Date(year, month - 1, 1).toLocaleDateString(locale, { month: 'short', year: 'numeric' });
  }

  private statusLabel(statusName: string): string {
    const key = `ORDERS.STATUS.${statusName}`;
    const translated = this.translate.instant(key);
    return translated === key ? statusName : translated;
  }
}
