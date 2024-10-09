using Microsoft.EntityFrameworkCore;

namespace Rolling.Models;

public class ApplicationContextDb : DbContext
{
    public DbSet<UserModel> UserModels => Set<UserModel>();

    public ApplicationContextDb() => Database.EnsureCreated();

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;DataBase=RollingBase;Trusted_Connection=True;");
    }
}