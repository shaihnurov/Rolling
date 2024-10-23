using Microsoft.AspNetCore.SignalR;
using Rolling.Models;
using ServerSignal.Hub;
using ServerSignal.SubscribeTableDependencies;
using ServerSignal.SubscribeTableDependencies;
using TableDependency.SqlClient;
using TableDependency.SqlClient.Base.Enums;

public class SubscribeRentalCarsTableDependency : ISubscribeTableDependency
{
    private SqlTableDependency<CarsRentalModels> _tableDependency;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<RentalCarHub> _hubContext;

    public SubscribeRentalCarsTableDependency(IServiceScopeFactory scopeFactory, IHubContext<RentalCarHub> hubContext)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
    }
    
    public void SubscribeTableDependency(string connectionString)
    {
        _tableDependency = new SqlTableDependency<CarsRentalModels>(connectionString);
        _tableDependency.OnChanged += TableDependency_OnChanged;
        _tableDependency.OnError += TableDependency_OnError;
        _tableDependency.Start();
    }

    private void TableDependency_OnChanged(object sender, TableDependency.SqlClient.Base.EventArgs.RecordChangedEventArgs<CarsRentalModels> e)
    {
        if (e.ChangeType != ChangeType.None)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                if (e.ChangeType == ChangeType.Update || e.ChangeType == ChangeType.Insert)
                {
                    _hubContext.Clients.All.SendAsync("UpdateCars", e.Entity);
                }
                else if (e.ChangeType == ChangeType.Delete)
                {
                    _hubContext.Clients.All.SendAsync("DeleteCars", e.Entity.Id);
                }
            }
        }
    }

    private void TableDependency_OnError(object sender, TableDependency.SqlClient.Base.EventArgs.ErrorEventArgs e)
    {
        Console.WriteLine($"{nameof(CarsRentalModels)} SqlTableDependency error: {e.Error.Message}");
    }
}