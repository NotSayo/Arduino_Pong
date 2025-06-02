using System.IO.Ports;
using JoystickSerial;

namespace PongApp.Coordinates;

using System.Collections.ObjectModel;
using System.ComponentModel;

public class CoordinateVM : INotifyPropertyChanged
{
    private SerialConnection? _serialConnection;
    public CancellationTokenSource? TokenSource { get; set; }
    private Task? SerialTask { get; set; }
    public ObservableCollection<PortsClass> Ports { get; private set; } = new();


    public WindowSize CanvasSize { get; } = new()
    {
        Width = 750,
        Height = 750
    };

    private JoyStickPosition _position = new JoyStickPosition(0,0);
    public JoyStickPosition Position
    {
        get => _position;
        set
        {
            _position = value;
            OnPropertyChanged(nameof(Position));
        }
    }

    private SerialStatus _status;

    public SerialStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged(nameof(Status));
        }
    }

    public CoordinateVM()
    {
        InitializeSerialConnection();
        GetAllPorts();
    }

    public JoyStickPosition AdjustPosition(JoyStickPosition position)
    {
        return new JoyStickPosition(
            position.X * CanvasSize.Width / 1000,
            position.Y * CanvasSize.Height / 1000);
    }

    public void InitializeSerialConnection()
    {
        if (_serialConnection != null)
        {
            _serialConnection.Dispose();
        }
        _serialConnection = new SerialConnection();
        _serialConnection.OnDataReceived += ReceiveData;
        _serialConnection.SerialStatusChanged += (status) => Status = status;
    }

    public void GetAllPorts()
    {
        if(_serialConnection is null)
            InitializeSerialConnection();
        Ports.Clear();
        foreach (var port in _serialConnection!.GetAllPorts())
            Ports.Add(new PortsClass { Name = port });
    }

    public void SubmitPort()
    {
        if (_serialConnection is null)
            InitializeSerialConnection();
        if (Ports.All(p => !p.IsPortChecked))
            return;
        var port = Ports.First(p => p.IsPortChecked);
        Console.WriteLine(port.Name);
        _serialConnection!.InitializePort(port.Name, 9600, 8, Parity.None, StopBits.One);
        TokenSource = new CancellationTokenSource();
        SerialTask = Task.Run(() => _serialConnection.Start(TokenSource.Token), TokenSource.Token);
    }

    private void ReceiveData(JoyStickPosition position)
    {
        Position = AdjustPosition(position);
        Console.WriteLine($"X: {position.X}, Y: {position.Y}");
    }

    #region OnPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    #endregion
}