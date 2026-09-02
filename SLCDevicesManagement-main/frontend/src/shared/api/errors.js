/**
 * ASP.NET ProblemDetails + FluentValidation suelen devolver:
 * { title, status, errors: { Nombre: ["..."] } }
 */
export function getValidationErrors(error) {
  const bag = error?.response?.data?.errors;

  if (!bag || typeof bag !== 'object' || Array.isArray(bag)) {
    return {};
  }

  return Object.fromEntries(
    Object.entries(bag).map(([field, messages]) => [
      field.charAt(0).toLowerCase() + field.slice(1),
      Array.isArray(messages) ? messages[0] : String(messages),
    ]),
  );
}

export function getApiErrorMessage(error) {
  if (!error) {
    return 'Ocurrió un error al procesar la solicitud.';
  }

  if (error.response) {
    const data = error.response.data;
    const fieldErrors = getValidationErrors(error);
    const firstField = Object.values(fieldErrors)[0];

    if (firstField) {
      return firstField;
    }

    return (
      data?.detail ||
      data?.message ||
      data?.title ||
      `La API respondió con HTTP ${error.response.status}.`
    );
  }

  if (error.request) {
    return 'No se pudo conectar con la API. Verifica VITE_API_URL y que el backend esté en ejecución.';
  }

  return error.message || 'Error inesperado.';
}
