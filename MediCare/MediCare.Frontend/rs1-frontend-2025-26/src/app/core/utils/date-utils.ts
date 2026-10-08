/**
 * Converts a Date to "yyyy-MM-dd" in LOCAL time.
 *
 * Do not use date.toISOString() for datepicker dates:
 * toISOString() works in UTC, so in Bosnia (UTC+1/+2) local midnight becomes the previous day.
 */
export function toLocalDateString(date: Date): string {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const d = String(date.getDate()).padStart(2, '0');
  return `${y}-${m}-${d}`;
}

/** Today at midnight (local time). */
export function startOfToday(): Date {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return today;
}
