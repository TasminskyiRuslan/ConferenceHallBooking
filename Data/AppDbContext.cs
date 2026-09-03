using ConferenceHallBookingApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBookingApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<HallOption> HallOptions => Set<HallOption>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingOption> BookingOptions => Set<BookingOption>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Hall>(entity =>
        {
            entity.HasIndex(h => h.Name).IsUnique();
            entity.Property(h => h.BaseHourlyRate).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.Property(o => o.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(b => b.TotalPrice).HasPrecision(18, 2);

            entity.HasOne(b => b.Hall)
                .WithMany(h => h.Bookings)
                .HasForeignKey(b => b.HallId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HallOption>(entity =>
        {
            entity.HasKey(ho => new { ho.HallId, ho.OptionId });

            entity.HasOne(ho => ho.Hall)
                .WithMany(h => h.HallOptions)
                .HasForeignKey(ho => ho.HallId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ho => ho.Option)
                .WithMany(o => o.HallOptions)
                .HasForeignKey(ho => ho.OptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookingOption>(entity =>
        {
            entity.HasKey(bo => new { bo.BookingId, bo.OptionId });

            entity.HasOne(bo => bo.Booking)
                .WithMany(b => b.BookingOptions)
                .HasForeignKey(bo => bo.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(bo => bo.Option)
                .WithMany()
                .HasForeignKey(bo => bo.OptionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}