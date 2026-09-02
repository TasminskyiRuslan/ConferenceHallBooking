using ConferenceHallBookingApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBookingApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<HallService> HallServices => Set<HallService>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingService> BookingServices => Set<BookingService>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Hall>(entity =>
        {
            entity.HasIndex(h => h.Name).IsUnique();
            entity.Property(h => h.BaseHourlyRate).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.Property(s => s.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.Property(b => b.TotalPrice).HasPrecision(18, 2);

            entity.HasOne(b => b.Hall)
                .WithMany(h => h.Bookings)
                .HasForeignKey(b => b.HallId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HallService>(entity =>
        {
            entity.HasKey(hs => new { hs.HallId, hs.ServiceId });

            entity.HasOne(hs => hs.Hall)
                .WithMany(h => h.HallServices)
                .HasForeignKey(hs => hs.HallId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(hs => hs.Service)
                .WithMany(s => s.HallServices)
                .HasForeignKey(hs => hs.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookingService>(entity =>
        {
            entity.HasKey(bs => new { bs.BookingId, bs.ServiceId });

            entity.HasOne(bs => bs.Booking)
                .WithMany(b => b.BookingServices)
                .HasForeignKey(bs => bs.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(bs => bs.Service)
                .WithMany()
                .HasForeignKey(bs => bs.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}