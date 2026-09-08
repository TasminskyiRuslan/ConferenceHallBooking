using ConferenceHallBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ConferenceHallBooking.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Booking"/> entity.
/// Applies DateTimeOffset → DateTime converter for SQLite compatibility.
/// </summary>
public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        var dateTimeOffsetToUtcConverter = new ValueConverter<DateTimeOffset, DateTime>(
            v => v.UtcDateTime,
            v => new DateTimeOffset(v, TimeSpan.Zero));

        builder.HasKey(b => b.Id);

        builder.Property(b => b.TotalPrice)
            .HasPrecision(18, 2);

        builder.Property(b => b.StartTime)
            .IsRequired()
            .HasConversion(dateTimeOffsetToUtcConverter);

        builder.Property(b => b.EndTime)
            .IsRequired()
            .HasConversion(dateTimeOffsetToUtcConverter);

        builder.Property(b => b.CreatedAtUtc)
            .IsRequired()
            .HasConversion(dateTimeOffsetToUtcConverter);

        builder.HasOne(b => b.Hall)
            .WithMany(h => h.Bookings)
            .HasForeignKey(b => b.HallId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => new { b.HallId, b.StartTime, b.EndTime });

        builder.Navigation(b => b.BookingOptions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
