export function gravatarUrl(email: string, size: number = 80): string {
  const normalizedEmail = email.trim().toLowerCase();

  return `http://www.gravatar.com/avatar/${normalizedEmail}?d=identicon&s=${size}`;
}
