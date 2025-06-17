using Bookify.Domain.Apartments;
using Bookify.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookify.Infrastructure.Configurations;

internal sealed class ApartmentConfiguration : IEntityTypeConfiguration<Apartment>
{
    public void Configure(EntityTypeBuilder<Apartment> builder)
    {
        builder.ToTable("Apartments");

        builder.HasKey(x => x.Id);

        builder.OwnsOne(apartment => apartment.Address);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasConversion(name => name.Value, value => new Name(value));

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(2000)
            .HasConversion(description => description.Value, value => new Description(value));

        builder.OwnsOne(apartment => apartment.Price, priceBuilder =>
        {
            priceBuilder.Property(money => money.Currency)
                .HasConversion(currency => currency.Code, value => Currency.FromCode(value));
        });

        builder.OwnsOne(apartment => apartment.CleaningFee, cleaningFeeBuilder =>
        {
            cleaningFeeBuilder.Property(money => money.Currency)
                .HasConversion(currency => currency.Code, value => Currency.FromCode(value));
        });

        builder.Property<uint>("Version").IsRowVersion();
    }
}
