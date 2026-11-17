import { getPasswordStrength } from './password-strength';

describe('getPasswordStrength', () => {
  it('shows nothing when no password is typed', () => {
    expect(getPasswordStrength('')).toEqual({ score: 0, label: '' });
    expect(getPasswordStrength(null)).toEqual({ score: 0, label: '' });
  });

  it('rates a very short password as weak even with many character types', () => {
    expect(getPasswordStrength('aB1!').score).toBe(1);
    expect(getPasswordStrength('aB1!').label).toBe('Slaba');
  });

  it('gives one point per rule: length, upper+lower, digit, special character', () => {
    expect(getPasswordStrength('abcdefgh').score).toBe(1);      // length only
    expect(getPasswordStrength('abcdefgh1').score).toBe(2);     // + digit
    expect(getPasswordStrength('Abcdefgh1').score).toBe(3);     // + upper and lower case
    expect(getPasswordStrength('Abcdefgh1!').score).toBe(4);    // + special character
    expect(getPasswordStrength('Abcdefgh1!').label).toBe('Jaka');
  });
});
