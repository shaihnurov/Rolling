using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;

namespace Rolling.ViewModels;

public class HomeViewModel : ObservableObject
{
    private HubConnection _connection;
    private readonly MainWindowViewModel _mainWindowViewModel;

    private ObservableCollection<string> _chatBox;
    private string _login;
    private string _message;

    public ObservableCollection<string> ChatBox
    {
        get => _chatBox;
        set => SetProperty(ref _chatBox, value);
    }
    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public RelayCommand SendCommand { get; set; }

    public HomeViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
        ChatBox = new ObservableCollection<string>();
        InitializeConnection();

        SendCommand = new RelayCommand(Send);
    }

    private async void InitializeConnection()
    {
        try
        {
            _connection = new HubConnectionBuilder().WithUrl("https://localhost:7160/chat").Build();

            _connection.On<string, string>("ReceiveChat", (user, message) =>
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    var newMessage = $"{user}: {message}";
                    ChatBox.Insert(0, newMessage);
                });
            });

            await Test();
        }
        catch (HttpRequestException ex)
        {
            _mainWindowViewModel.Notification("Server Error", $"{ex.StatusCode} {ex.Message}", true, false, 3, true);
        }
        catch (SocketException ex)
        {
            _mainWindowViewModel.Notification("Server Error(socket)", $"{ex.SocketErrorCode} {ex.Message}", true, false, 3, true);
        }
    }

    private async Task Test()
    {
        try
        {
            await _connection.StartAsync();
            ChatBox.Add("Вы успешно вошли в чат");
        }
        catch (Exception ex)
        {
            ChatBox.Add(ex.Message);
        }
    }
    private async void Send()
    {
        try
        {
            await _connection.InvokeAsync("Send", Login, Message);
        }
        catch (Exception ex)
        {
            ChatBox.Add(ex.Message);
        }
    }
}