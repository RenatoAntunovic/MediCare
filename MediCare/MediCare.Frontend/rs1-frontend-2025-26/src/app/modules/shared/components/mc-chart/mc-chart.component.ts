import {
  Component,
  DestroyRef,
  ElementRef,
  computed,
  effect,
  inject,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import {
  BarController,
  BarElement,
  CategoryScale,
  Chart,
  ChartConfiguration,
  Legend,
  LinearScale,
  LineController,
  LineElement,
  PointElement,
  Tooltip,
  TooltipItem,
} from 'chart.js';
import { ThemeService } from '../../../../core/services/theme.service';
import { formatCount, formatMoney } from '../../../../core/utils/locale-format';

// Register only the Chart.js parts we use (keeps the bundle small)
Chart.register(
  BarController,
  BarElement,
  LineController,
  LineElement,
  PointElement,
  CategoryScale,
  LinearScale,
  Tooltip,
  Legend
);

export interface McChartSeries {
  label: string;
  data: number[];
  /** Fixed palette slot – the same entity always keeps the same color */
  slot: 1 | 2;
}

/**
 * Thin wrapper around Chart.js (plain JS charts).
 *
 * - Colors come from CSS tokens (--mc-chart-1/2, text, border), so light and dark mode both work;
 *   the chart is rebuilt when the theme changes.
 * - Under every chart there is a "show as table" view with the same numbers (accessibility).
 */
@Component({
  selector: 'app-mc-chart',
  standalone: true,
  imports: [TranslateModule],
  templateUrl: './mc-chart.component.html',
  styleUrl: './mc-chart.component.scss',
})
export class McChartComponent {
  type = input<'line' | 'bar'>('bar');
  labels = input.required<string[]>();
  series = input.required<McChartSeries[]>();
  /** Horizontal bars – better for long category names (treatments, medicines) */
  horizontal = input(false);
  /** Format values as money (KM) */
  money = input(false);
  /** Short description for screen readers */
  ariaLabel = input('');

  private theme = inject(ThemeService);
  private translate = inject(TranslateService);
  /** Selected language; a change re-renders the chart so ticks and tooltips use the new locale */
  private lang = signal(this.translate.currentLang || 'bs');
  private canvas = viewChild.required<ElementRef<HTMLCanvasElement>>('canvas');
  private probe = viewChild.required<ElementRef<HTMLElement>>('probe');
  private chart: Chart | null = null;

  /** Horizontal bar charts grow with the number of rows, others have a fixed height */
  height = computed(() =>
    this.horizontal() ? Math.max(180, this.labels().length * 40 + 40) : 300
  );

  constructor() {
    effect(() => {
      // Re-run when data or theme changes
      const config = {
        type: this.type(),
        labels: this.labels(),
        series: this.series(),
        horizontal: this.horizontal(),
        money: this.money(),
      };
      this.theme.isDark();
      this.lang();

      // Wait one frame so the theme class on <body> is already applied before reading colors
      const frame = requestAnimationFrame(() => this.render(config));
      return () => cancelAnimationFrame(frame);
    });

    this.translate.onLangChange
      .pipe(takeUntilDestroyed())
      .subscribe((e) => this.lang.set(e.lang));

    inject(DestroyRef).onDestroy(() => this.chart?.destroy());
  }

  /** Numbers (axis ticks, tooltips, table view) follow the language selected in the app */
  format(value: number): string {
    return this.money() ? formatMoney(value, this.lang()) : formatCount(value, this.lang());
  }

  private render(c: {
    type: 'line' | 'bar';
    labels: string[];
    series: McChartSeries[];
    horizontal: boolean;
    money: boolean;
  }): void {
    const color = (token: string) => this.resolveColor(token);
    const text = color('--mc-text');
    const muted = color('--mc-text-muted');
    const grid = color('--mc-border');
    const surface = color('--mc-surface');
    const font = getComputedStyle(this.canvas().nativeElement).fontFamily;

    const datasets = c.series.map((s) => {
      const seriesColor = color(`--mc-chart-${s.slot}`);
      return c.type === 'line'
        ? {
            label: s.label,
            data: s.data,
            borderColor: seriesColor,
            backgroundColor: seriesColor,
            borderWidth: 2,
            pointRadius: 4,
            pointHoverRadius: 6,
            pointBorderColor: surface,
            pointBorderWidth: 2,
            tension: 0.25,
          }
        : {
            label: s.label,
            data: s.data,
            backgroundColor: seriesColor,
            borderRadius: 4,
            borderSkipped: 'start' as const,
            maxBarThickness: 28,
          };
    });

    const valueAxis = {
      beginAtZero: true,
      grid: { color: grid },
      border: { display: false },
      ticks: {
        color: muted,
        precision: c.money ? undefined : 0,
        callback: (v: string | number) => this.format(Number(v)),
      },
    };
    const categoryAxis = {
      grid: { display: false },
      border: { color: grid },
      ticks: { color: muted },
    };

    const config: ChartConfiguration = {
      type: c.type,
      data: { labels: c.labels, datasets: datasets as any },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: { duration: 300 },
        indexAxis: c.horizontal ? 'y' : 'x',
        interaction: c.type === 'line' ? { mode: 'index', intersect: false } : { mode: 'nearest', intersect: true },
        font: { family: font },
        scales: c.horizontal
          ? { x: valueAxis, y: categoryAxis }
          : { x: categoryAxis, y: valueAxis },
        plugins: {
          // A single series needs no legend – the card title names it
          legend: {
            display: c.series.length > 1,
            position: 'top',
            align: 'end',
            labels: { color: text, usePointStyle: true, boxWidth: 8, boxHeight: 8, font: { family: font } },
          },
          tooltip: {
            backgroundColor: surface,
            titleColor: text,
            bodyColor: text,
            borderColor: grid,
            borderWidth: 1,
            padding: 10,
            usePointStyle: true,
            callbacks: {
              label: (item: TooltipItem<'line' | 'bar'>) => `${item.dataset.label}: ${this.format(Number(item.raw))}`,
            },
          },
        },
      } as any,
    };

    this.chart?.destroy();
    this.chart = new Chart(this.canvas().nativeElement, config);
  }

  /** CSS custom properties hold light-dark(...) values – let the browser resolve them to a real color */
  private resolveColor(token: string): string {
    const el = this.probe().nativeElement;
    el.style.color = `var(${token})`;
    return getComputedStyle(el).color;
  }
}
