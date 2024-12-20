﻿using Microsoft.EntityFrameworkCore;
using Rolling.Models;

namespace ServerSignal.Models
{
    public class ApplicationContextDb : DbContext
    {
        public DbSet<UserModel> UserModels => Set<UserModel>();
        public DbSet<CarsRentalModels> CarsRentalModels => Set<CarsRentalModels>();
        public DbSet<ListRentalsModel> ListRentalsModels => Set<ListRentalsModel>();

        public ApplicationContextDb(DbContextOptions<ApplicationContextDb> options) : base(options) { }
    }
}