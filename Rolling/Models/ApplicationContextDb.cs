using Microsoft.EntityFrameworkCore;

namespace Rolling.Models;

public class ApplicationContextDb : DbContext
{
    public DbSet<UserModel> UserModels => Set<UserModel>();
    public DbSet<CarsRentalModel> CarsRentalModels => Set<CarsRentalModel>();
    
    public ApplicationContextDb(DbContextOptions<ApplicationContextDb> options) : base(options)
    {
        // Удаляем EnsureCreated, если планируете использовать миграции
        // Database.EnsureCreated(); // Используйте только если не планируете миграции
    }
}