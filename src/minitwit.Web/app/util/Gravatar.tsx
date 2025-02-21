import { createHash } from 'crypto';

export function gravatarUrl(email: string, size: number = 80): string {
  const normalizedEmail = email.trim().toLowerCase();

  const hash = createHash('md5').update(normalizedEmail).digest('hex');

  return `http://www.gravatar.com/avatar/${hash}?d=identicon&s=${size}`;
}
