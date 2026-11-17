import { formatDate, formatNumber, registerLocaleData } from '@angular/common';
import localeBs from '@angular/common/locales/bs';

// Angular ships only the en-US locale data. Bosnian ("bs") must be registered once,
// otherwise formatNumber / formatDate cannot format with it.
registerLocaleData(localeBs);

/**
 * Angular locale id for the language selected in the app (the ngx-translate code: "bs" or "en").
 * Anything that is not English falls back to Bosnian, the default language of the app.
 */
export function localeFor(lang: string | null | undefined): string {
  return lang === 'en' ? 'en-US' : 'bs';
}

/** Amount with thousands and decimals: bs "KM 1.234,50", en "KM 1,234.50" */
export function formatMoney(value: number, lang: string | null | undefined): string {
  return 'KM ' + formatNumber(value, localeFor(lang), '1.2-2');
}

/** Plain number (counts, quantities): bs "1.234", en "1,234" */
export function formatCount(value: number, lang: string | null | undefined): string {
  return formatNumber(value, localeFor(lang), '1.0-2');
}

/** Short month with year for chart axes, e.g. bs "mar 2026", en "Mar 2026" */
export function formatMonthYear(year: number, month: number, lang: string | null | undefined): string {
  return formatDate(new Date(year, month - 1, 1), 'LLL y', localeFor(lang));
}
