import md5 from "crypto-js/md5";

export function gravatarUrl(email: string, size: number = 80): string {
  const normalizedEmail = email.trim().toLowerCase();
  const hash = md5(normalizedEmail).toString();
  return `http://www.gravatar.com/avatar/${hash}?d=identicon&s=${size}`;
}
