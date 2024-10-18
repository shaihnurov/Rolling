using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Rolling.Models;
using Server.Model;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Enums;
using TableDependency.SqlClient.Base.EventArgs;

namespace Server.Hub
{
    public class CarHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly ApplicationContextDb _db;
        private readonly SqlTableDependency<CarsRentalModel> _tableDependency;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<CarHub> _hubContext;
        private bool _isActive = true;

        public CarHub(ApplicationContextDb applicationContextDb, IHubContext<CarHub> hubContext, IConfiguration config)
        {
            _db = applicationContextDb;
            _configuration = config;
            _hubContext = hubContext;

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            _tableDependency = new SqlTableDependency<CarsRentalModel>(connectionString, "CarsRentalModels");

            _tableDependency.OnChanged += TableDependency_OnChanged;
            _tableDependency.Start();
        }

        public async Task GetCars()
        {
            var cars = await _db.CarsRentalModels.ToListAsync();
            await Clients.Caller.SendAsync("ReceiveCars", cars);
        }
        private async void TableDependency_OnChanged(object sender, RecordChangedEventArgs<CarsRentalModel> e)
        {
            if (!_isActive) return;

            switch (e.ChangeType)
            {
                case ChangeType.Insert:
                case ChangeType.Update:
                    var changedEntity = e.Entity;
                    await _hubContext.Clients.All.SendAsync("ReceiveCarUpdate", changedEntity);
                    break;

                case ChangeType.Delete:
                    await _hubContext.Clients.All.SendAsync("ReceiveCarDelete", e.Entity.Id);
                    break;
            }
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _isActive = false;

            try
            {
                if (_tableDependency != null)
                {
                    _tableDependency.OnChanged -= TableDependency_OnChanged;
                    _tableDependency.Stop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping TableDependency: {ex.Message}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}