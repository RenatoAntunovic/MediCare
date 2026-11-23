import { Pipe, PipeTransform, inject } from '@angular/core';
import { formatDate, formatNumber } from '@angular/common';
import { TranslateService } from '@ngx-translate/core';
import { localeFor } from '../../../core/utils/locale-format';

/**
 * Number in the format of the language selected in the UI (bs: "1.234,50", en: "1,234.50").
 * Replaces Angular's "number" pipe, which always uses the default en-US locale.
 * Not pure, so the value is re-formatted right after the language is switched.
 * Usage: {{ price | localNumber : '1.2-2' }} KM
 */
@Pipe({ name: 'localNumber', standalone: true, pure: false })
export class LocalNumberPipe implements PipeTransform {
  private translate = inject(TranslateService);

  transform(value: number | null | undefined, digitsInfo = '1.2-2'): string {
    if (value === null || value === undefined || isNaN(value)) return '';
    return formatNumber(value, localeFor(this.translate.currentLang), digitsInfo);
  }
}

/**
 * Date in the locale of the language selected in the UI (month names etc. follow the language).
 * Usage: {{ order.orderDate | localDate : 'dd.MM.yyyy HH:mm' }}
 */
@Pipe({ name: 'localDate', standalone: true, pure: false })
export class LocalDatePipe implements PipeTransform {
  private translate = inject(TranslateService);

  transform(value: string | Date | null | undefined, format = 'dd.MM.yyyy'): string {
    if (!value) return '';
    return formatDate(value, format, localeFor(this.translate.currentLang));
  }
}
