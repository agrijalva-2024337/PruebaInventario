import { useState } from 'react';

function readField(eventOrField) {
  if (eventOrField?.target) {
    const { name, type, checked, value } = eventOrField.target;
    return { name, value: type === 'checkbox' ? checked : value };
  }

  return { name: eventOrField.name, value: eventOrField.value };
}

export function useForm({ initialValues, validate }) {
  const [values, setValues] = useState(initialValues);
  const [errors, setErrors] = useState({});
  const [touched, setTouched] = useState({});

  function applyValidation(nextValues) {
    const nextErrors = validate ? validate(nextValues) : {};
    setErrors(nextErrors);
    return nextErrors;
  }

  function handleChange(eventOrField) {
    const { name, value } = readField(eventOrField);
    const nextValues = { ...values, [name]: value };
    setValues(nextValues);

    if (touched[name]) {
      applyValidation(nextValues);
    }
  }

  function handleBlur(eventOrField) {
    const { name } = readField(eventOrField);
    setTouched((current) => ({ ...current, [name]: true }));
    applyValidation(values);
  }

  function handleSubmit(onValid) {
    return (event) => {
      event.preventDefault();
      const allTouched = Object.fromEntries(Object.keys(values).map((key) => [key, true]));
      setTouched(allTouched);
      const nextErrors = applyValidation(values);

      if (Object.keys(nextErrors).length === 0) {
        onValid(values);
      }
    };
  }

  function reset(nextValues = initialValues) {
    setValues(nextValues);
    setErrors({});
    setTouched({});
  }

  function setFieldValue(name, value) {
    const nextValues = { ...values, [name]: value };
    setValues(nextValues);
    if (touched[name]) {
      applyValidation(nextValues);
    }
  }

  return {
    values,
    errors,
    touched,
    handleChange,
    handleBlur,
    handleSubmit,
    reset,
    setValues,
    setFieldValue,
  };
}
