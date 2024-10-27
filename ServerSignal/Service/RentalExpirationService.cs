using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ServerSignal.Models;

namespace ServerSignal.Service;

public class RentalExpirationService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private Timer _timer;

    public RentalExpirationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(CheckExpiredRentals, null, TimeSpan.Zero, TimeSpan.FromDays(1));
        return Task.CompletedTask;
    }

    private async void CheckExpiredRentals(object state)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContextDb>();

        var expiredRentals = await dbContext.ListRentalsModels.Where(x => x.EndDate <= DateTime.Now).ToListAsync();

        foreach (var rental in expiredRentals)
        {
            var sql = @"UPDATE CarsRentalModels SET Status = @Status WHERE Id = @Id";

            await dbContext.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@Status", false),
                new SqlParameter("@Id", rental.CarId)
            );
        
            dbContext.ListRentalsModels.Remove(rental);
        }
    
        await dbContext.SaveChangesAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}