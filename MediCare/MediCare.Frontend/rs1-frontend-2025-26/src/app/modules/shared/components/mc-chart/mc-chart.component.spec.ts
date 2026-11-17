import { TestBed } from '@angular/core/testing';
import { TranslateService } from '@ngx-translate/core';
import { Subject } from 'rxjs';
import { McChartComponent } from './mc-chart.component';
import { ThemeService } from '../../../../core/services/theme.service';

// Chart.js needs a real canvas, which jsdom does not have: the chart itself is replaced by a stub.
// The test is about the numbers the chart formats (axis ticks, tooltips and the table view use format()).
vi.mock('chart.js', () => {
  class Chart {
    static register() {}
    destroy() {}
  }
  const stub = class {};
  return {
    Chart, BarController: stub, BarElement: stub, CategoryScale: stub, Legend: stub, LinearScale: stub,
    LineController: stub, LineElement: stub, PointElement: stub, Tooltip: stub
  };
});

describe('McChartComponent (locale)', () => {
  let langChange: Subject<{ lang: string }>;

  const create = (money: boolean) => {
    const fixture = TestBed.createComponent(McChartComponent);
    fixture.componentRef.setInput('labels', ['Mar 2026']);
    fixture.componentRef.setInput('series', [{ label: 'A', data: [1234.5], slot: 1 }]);
    fixture.componentRef.setInput('money', money);
    return fixture.componentInstance;
  };

  beforeEach(() => {
    langChange = new Subject();
    TestBed.configureTestingModule({
      providers: [
        { provide: TranslateService, useValue: { currentLang: 'bs', onLangChange: langChange, instant: (k: string) => k, get: () => new Subject() } },
        { provide: ThemeService, useValue: { isDark: () => false } }
      ]
    }).overrideComponent(McChartComponent, {
      set: { template: '<span #probe></span><canvas #canvas></canvas>', imports: [] }
    });
  });

  it('formats amounts with the selected language', () => {
    const chart = create(true);
    expect(chart.format(1234.5)).toBe('KM 1.234,50');

    langChange.next({ lang: 'en' });
    expect(chart.format(1234.5)).toBe('KM 1,234.50');
  });

  it('formats plain counts with the selected language', () => {
    const chart = create(false);
    expect(chart.format(1234567)).toBe('1.234.567');

    langChange.next({ lang: 'en' });
    expect(chart.format(1234567)).toBe('1,234,567');
  });
});
