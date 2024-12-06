using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Rolling.Models;
using Rolling.Service;
using Rolling.ViewModels;
using Rolling.Views;

namespace Rolling;

public partial class App : Application
{
    private HubConnection _connection;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
            desktop.Exit += OnExit;
        }
        base.OnFrameworkInitializationCompleted();
    }

    private void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        if (_connection.State == HubConnectionState.Connected)
        {
            _connection.StopAsync(); 
            _connection.DisposeAsync();
        }
    }
}