namespace Bookify.Domain.Shared;

public record Money(decimal Amount, Currency Currency)
{
    public static Money operator +(Money first, Money second) =>
         first.Currency != second.Currency
             ? throw new ApplicationException("Cannot add two Money values with different currencies")
             : first with { Amount = first.Amount + second.Amount };

    public static Money Zero() => new(0, Currency.None);

    public static Money Zero(Currency currency) => new(0, currency);

    public bool IsZero() => this == Zero(Currency);
}