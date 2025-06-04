using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PongApp.Coordinates;
using PongApp.Windows;

namespace PongApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public CoordinateVM vm { get; set; }
    public MainWindow()
    {
        vm = new();
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

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Windows.Coordinates coordinatesWindow = new Windows.Coordinates(vm);
        coordinatesWindow.Show();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
    }

    private void ButonAccuracy_OnClick(object sender, RoutedEventArgs e)
    {
        Windows.Accuracy accuracyWindow = new Accuracy(vm);
        accuracyWindow.Show();
    }
}