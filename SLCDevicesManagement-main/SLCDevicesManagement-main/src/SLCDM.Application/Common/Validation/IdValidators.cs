using FluentValidation;

namespace SLCDM.Application.Common.Validation;

public static class IdValidators
{
    public static IRuleBuilderOptions<T, int> RequiredId<T>(
        this IRuleBuilder<T, int> ruleBuilder,
        string fieldName)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithMessage($"El campo {fieldName} es obligatorio y debe ser mayor a 0.");
    }

    public static IRuleBuilderOptions<T, int?> OptionalId<T>(
        this IRuleBuilder<T, int?> ruleBuilder,
        string fieldName)
    {
        return ruleBuilder
            .Must(id => !id.HasValue || id.Value > 0)
            .WithMessage($"El campo {fieldName} debe ser mayor a 0 cuando se informa.");
    }
}
