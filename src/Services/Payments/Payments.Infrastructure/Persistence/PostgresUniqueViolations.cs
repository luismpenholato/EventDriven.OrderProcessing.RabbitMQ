using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Payments.Infrastructure.Persistence;

public static class PostgresUniqueViolations
{
    public static bool IsUniqueViolation(DbUpdateException exception)
    {
        for (var inner = exception.InnerException; inner is not null; inner = inner.InnerException)
        {
            if (inner is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
                return true;
        }

        return false;
    }

    public static bool IsPaymentOrderIdUniqueViolation(DbUpdateException exception)
    {
        if (!IsUniqueViolation(exception))
            return false;

        for (var inner = exception.InnerException; inner is not null; inner = inner.InnerException)
        {
            if (inner is not PostgresException postgres)
                continue;

            if (postgres.ConstraintName?.Contains("OrderId", StringComparison.OrdinalIgnoreCase) == true)
                return true;

            if (postgres.MessageText.Contains("IX_payments_OrderId", StringComparison.OrdinalIgnoreCase)
                || postgres.MessageText.Contains("payments", StringComparison.OrdinalIgnoreCase)
                    && postgres.MessageText.Contains("OrderId", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
