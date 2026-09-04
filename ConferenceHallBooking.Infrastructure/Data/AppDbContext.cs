using System.Reflection;
using ConferenceHallBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHallBooking.Infrastructure.Data;

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

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}