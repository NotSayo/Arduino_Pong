using System.ComponentModel;
using System.Windows;
using JoystickSerial;
using PongApp.Coordinates;

namespace PongApp.Windows;

public partial class Accuracy : Window
{
    public CoordinateVM vm { get; set; }
    public Accuracy(CoordinateVM vm)
    {
        this.vm = vm;
        this.DataContext = vm;
        InitializeComponent();
        // vm.StartAccuracyGame();
    }


    protected override void OnClosing(CancelEventArgs e)
    {
        vm.StopAccuracyGame();
        base.OnClosing(e);
    }

    private void Start_Game(object sender, RoutedEventArgs e)
    {
        vm.StartAccuracyGame();
    }
}