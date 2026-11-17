import { formatCount, formatMoney, formatMonthYear, localeFor } from './locale-format';

describe('locale-format (statistics amounts and dates)', () => {
  it('formats the same amount with thousands and decimals in Bosnian and in English', () => {
    expect(formatMoney(1234.5, 'bs')).toBe('KM 1.234,50');
    expect(formatMoney(1234.5, 'en')).toBe('KM 1,234.50');
  });

  it('formats counts with the selected locale', () => {
    expect(formatCount(1234567, 'bs')).toBe('1.234.567');
    expect(formatCount(1234567, 'en')).toBe('1,234,567');
  });

  it('formats the month of a chart axis with the selected locale', () => {
    expect(formatMonthYear(2026, 3, 'en')).toBe('Mar 2026');
    expect(formatMonthYear(2026, 3, 'bs')).toContain('2026');
    expect(formatMonthYear(2026, 3, 'bs')).not.toBe(formatMonthYear(2026, 3, 'en'));
  });

  it('uses Bosnian when the language is missing or unknown (default language of the app)', () => {
    expect(localeFor(undefined)).toBe('bs');
    expect(localeFor('de')).toBe('bs');
    expect(localeFor('en')).toBe('en-US');
  });
});
