const LOWERS = 'abcdefghijkmnpqrstuvwxyz';
const UPPERS = 'ABCDEFGHJKLMNPQRSTUVWXYZ';
const DIGITS = '23456789';
const SYMBOLS = '!@#$%&*?';
const ALL = `${LOWERS}${UPPERS}${DIGITS}${SYMBOLS}`;

function pick(alphabet) {
  const index = crypto.getRandomValues(new Uint32Array(1))[0] % alphabet.length;
  return alphabet[index];
}

function shuffle(chars) {
  for (let i = chars.length - 1; i > 0; i -= 1) {
    const j = crypto.getRandomValues(new Uint32Array(1))[0] % (i + 1);
    const current = chars[i];
    chars[i] = chars[j];
    chars[j] = current;
  }
  return chars;
}

/** Misma idea que PasswordGenerator.cs: 12 chars, RNG, sin 0/O/1/l/I. */
export function generatePassword(length = 12) {
  const chars = [
    pick(UPPERS),
    pick(LOWERS),
    pick(DIGITS),
    pick(SYMBOLS),
    ...Array.from({ length: Math.max(0, length - 4) }, () => pick(ALL)),
  ];

  return shuffle(chars).join('');
}
