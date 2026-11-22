using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Payments.Infrastructure.Persistence;

namespace Payments.Infrastructure.Tests;

public sealed class PostgresUniqueViolationsTests
{
    [Fact]
    public void IsPaymentOrderIdUniqueViolation_WhenConstraintMentionsOrderId_ReturnsTrue()
    {
        var postgres = new PostgresException(
            "duplicate key value violates unique constraint \"IX_payments_OrderId\"",
            "ERROR",
            "ERROR",
            PostgresErrorCodes.UniqueViolation);

        var exception = new DbUpdateException("Failed to save", postgres);

        PostgresUniqueViolations.IsPaymentOrderIdUniqueViolation(exception).Should().BeTrue();
        PostgresUniqueViolations.IsUniqueViolation(exception).Should().BeTrue();
    }

    [Fact]
    public void IsPaymentOrderIdUniqueViolation_WhenOtherUniqueConstraint_ReturnsFalse()
    {
        var postgres = new PostgresException(
            "duplicate key value violates unique constraint \"PK_processed_messages\"",
            "ERROR",
            "ERROR",
            PostgresErrorCodes.UniqueViolation);

        var exception = new DbUpdateException("Failed to save", postgres);

        PostgresUniqueViolations.IsUniqueViolation(exception).Should().BeTrue();
        PostgresUniqueViolations.IsPaymentOrderIdUniqueViolation(exception).Should().BeFalse();
    }
}
