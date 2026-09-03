using ConferenceHallBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceHallBooking.Infrastructure.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.TotalPrice)
            .HasPrecision(18, 2);

        builder.Property(b => b.StartTime)
            .IsRequired();

        builder.Property(b => b.EndTime)
            .IsRequired();

        builder.Property(b => b.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(b => b.Hall)
            .WithMany(h => h.Bookings)
            .HasForeignKey(b => b.HallId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(b => b.BookingOptions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}