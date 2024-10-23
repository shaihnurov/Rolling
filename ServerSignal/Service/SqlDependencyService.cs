using Rolling.Models;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Delegates;

namespace ServerSignal.Service
{
    public class SqlDependencyService
    {
        private readonly SqlTableDependency<CarsRentalModels> _carsRentalTableDependencyService;

        public SqlDependencyService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            _carsRentalTableDependencyService = new SqlTableDependency<CarsRentalModels>(connectionString, "CarsRentalModels");
        }

        public void Start()
        {
            _carsRentalTableDependencyService.Start();
        }

        public event ChangedEventHandler<CarsRentalModels> CarsRentalChanged
        {
            add => _carsRentalTableDependencyService.OnChanged += value;
            remove => _carsRentalTableDependencyService.OnChanged -= value;
        }
    }
}