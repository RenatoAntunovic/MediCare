/**
 * Password strength meter logic (only a hint for the user, the server rules stay the same).
 * One point for each of: 8+ characters, lower and upper case letters, a digit, a special character.
 */
export interface PasswordStrength {
  /** 0 = nothing typed, 1 = weak … 4 = strong. Also the number of filled bars. */
  score: 0 | 1 | 2 | 3 | 4;
  label: string;
}

const LABELS = ['', 'Slaba', 'Srednja', 'Dobra', 'Jaka'] as const;

export function getPasswordStrength(password: string | null | undefined): PasswordStrength {
  if (!password) {
    return { score: 0, label: '' };
  }

  let points = 0;
  if (password.length >= 8) points++;
  if (/[a-z]/.test(password) && /[A-Z]/.test(password)) points++;
  if (/\d/.test(password)) points++;
  if (/[^A-Za-z0-9]/.test(password)) points++;

  // A very short password is always "Slaba", however many character types it has
  if (password.length < 6) points = Math.min(points, 1);

  // At least one bar is shown as soon as the user types something
  const score = Math.max(points, 1) as PasswordStrength['score'];
  return { score, label: LABELS[score] };
}
