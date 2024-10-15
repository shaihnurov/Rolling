using Microsoft.EntityFrameworkCore;

namespace Rolling.Models;

public class ApplicationContextDb : DbContext
{
    public DbSet<UserModel> UserModels => Set<UserModel>();
    public DbSet<CarsRentalModel> CarsRentalModels => Set<CarsRentalModel>();

    public ApplicationContextDb() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;DataBase=BaseRolling;Trusted_Connection=True;");
    }
}