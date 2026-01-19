import type { IdentityError } from '@/types/chat'
import type { AxiosError } from 'axios'

// type guard: garante em runtime e informa o TS em compile time
export function isAxiosError<T = unknown>(err: unknown): err is AxiosError<T> {
  return (
    typeof err === 'object' &&
    err !== null &&
    (err as { isAxiosError: boolean }).isAxiosError === true
  )
}

export function isIdentityErrorArray(data: unknown): data is IdentityError[] {
  return (
    Array.isArray(data) &&
    data.every(
      (x) =>
        typeof x === 'object' &&
        x !== null &&
        typeof (x as { description: string }).description === 'string',
    )
  )
}
