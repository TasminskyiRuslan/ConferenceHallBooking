using ConferenceHallBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Реєструємо ініціалізатор у DI-контейнері
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. Дістаємо IDbInitializer із DI та викликаємо SeedAsync без передачі context
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.SeedAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();