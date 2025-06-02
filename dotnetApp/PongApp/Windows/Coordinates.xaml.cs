using System.ComponentModel;
using System.Windows;
using JoystickSerial;
using PongApp.Coordinates;

namespace PongApp.Windows;

public partial class Coordinates : Window
{
    public CoordinateVM vm { get; set; }
    public Coordinates(CoordinateVM vm)
    {
        this.vm = vm;
        this.DataContext = vm;
        InitializeComponent();
    }

    private void SubmitPort(object sender, RoutedEventArgs e)
    {
        vm.SubmitPort();
    }

    private void RefreshConnection(object sender, RoutedEventArgs e)
    {
        vm.InitializeSerialConnection();
        vm.GetAllPorts();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (vm.TokenSource != null)
        {
            vm.TokenSource.Cancel();
            vm.TokenSource.Dispose();
        }
    }
}