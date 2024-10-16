using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Net.Http;

namespace Rolling.Views;

public partial class HomeView : UserControl
{
    public HomeView()
    {
        InitializeComponent();
    }
}