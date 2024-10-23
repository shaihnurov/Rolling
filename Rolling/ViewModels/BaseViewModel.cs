using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Rolling.Service;

namespace Rolling.ViewModels;

public abstract class BaseViewModel : ObservableObject, IServerConnectionHandler
{
    protected HubConnection _hubConnection;
    private readonly string _signalRUrl;
    
    protected BaseViewModel(string signalRUrl)
    {
        _signalRUrl = signalRUrl;
    }

    public virtual async Task ConnectToSignalR()
    {
        if (_hubConnection != null)
        {
            Console.WriteLine("Already connected or connecting. Please wait...");
            return;
        }

        try
        {
            _hubConnection = new HubConnectionBuilder().WithUrl($"https://localhost:7296/{_signalRUrl}").WithAutomaticReconnect(new[]
            {
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(20),
            }).Build();

            _hubConnection.Reconnecting += (exception) =>
            {
                Console.WriteLine($"Reconnecting to {_signalRUrl}");
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += (connectionId) =>
            {
                Console.WriteLine($"Reconnected. ConnectionId: {connectionId}");
                return Task.CompletedTask;
            };

            _hubConnection.Closed += async (exception) =>
            {
                Console.WriteLine($"Connection closed due to an error: {exception?.Message}");
                await Task.Delay(1000);
                await _hubConnection.StartAsync();
            };

            await _hubConnection.StartAsync();
            Console.WriteLine("Connected successfully.");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Request Error: {ex.Message}");
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Socket Error: {ex.Message}");
        }
        catch (HubException ex)
        {
            Console.WriteLine($"SignalR Hub Error: {ex.Message}");
        }
        catch (TimeoutException ex)
        {
            Console.WriteLine($"Timeout Error: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Invalid Operation: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected Error: {ex.Message}");
        }
    }
}