import { getApiErrorMessage } from '@/shared/api/errors';

export function getErrorMessage(error) {
  return getApiErrorMessage(error);
}
