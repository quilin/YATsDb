using Cronos;
using FluentValidation;

namespace YATsDB.Server.Infrastructure.Validation;

internal static class CustomValidators
{
    public static void MustByCronExpression<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        ruleBuilder.Must(t => CronExpression.TryParse(t, out _))
            .WithMessage("Expression is not valid cron expression.");
    }
}