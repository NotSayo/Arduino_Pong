using System.Diagnostics;
using System.IO.Ports;
using System.Windows;
using JoystickSerial;
using PongApp.Movement;

namespace PongApp.Coordinates;

using System.Collections.ObjectModel;
using System.ComponentModel;

public class CoordinateVM : INotifyPropertyChanged
{
    private SerialConnection? _serialConnection;
    public CancellationTokenSource? TokenSource { get; set; }
    private Task? SerialTask { get; set; }
    public ObservableCollection<PortsClass> Ports { get; private set; } = new();


    public ItemSize CanvasSize { get; } = new()
    {
        Width = 900,
        Height = 900
    };

    public ItemSize PlayerSize { get; } = new()
    {
        Width = 15,
        Height = 15
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

    public IMovement Movement { get; set; } = new SetPosition();

    public CoordinateVM()
    {
        InitializeSerialConnection();
        GetAllPorts();
        Movement.Speed = Speed;
    }


    public JoyStickPosition AdjustPosition(JoyStickPosition position)
    {
        return new JoyStickPosition(
            (position.X * CanvasSize.Width / 1000) - PlayerSize.Width / 2,
            (position.Y * CanvasSize.Height / 1000) - PlayerSize.Height / 2);
    }

    public void InitializeSerialConnection()
    {
        if (_serialConnection is not null)
            _serialConnection.Dispose();

        _serialConnection = new SerialConnection();
        _serialConnection.OnDataReceived += ReceiveData;
        _serialConnection.SerialStatusChanged += (status) => Status = status;
        _serialConnection.OnErrorProcessingData += (e, errorMessage) => Console.WriteLine($"Error: {errorMessage}\n{e.Message}");
    }

    public void GetAllPorts()
    {
        if(_serialConnection is null)
            InitializeSerialConnection();
        Ports.Clear();
        foreach (var port in _serialConnection!.GetAllPorts())
            Ports.Add(new PortsClass { Name = port });
        OnPropertyChanged(nameof(Ports));
    }

    public void SubmitPort()
    {
        if (_serialConnection is null)
            InitializeSerialConnection();
        if (Ports.All(p => !p.IsPortChecked))
            return;
        if(SerialTask is not null )
        {
            if(!SerialTask.IsCompleted)
                TokenSource!.Cancel();
            while (!SerialTask.IsCompleted) ;
            SerialTask.Dispose();
            SerialTask = null;
        }
        var port = Ports.First(p => p.IsPortChecked);
        _serialConnection!.InitializePort(port.Name, 9600, 8, Parity.None, StopBits.One);
        TokenSource = new CancellationTokenSource();
        SerialTask = Task.Run(() => _serialConnection.Start(TokenSource.Token), TokenSource.Token);
    }

    private void ReceiveData(JoyStickPosition position)
    {
        var newPosition = Movement.CalculatePosition(position);
        Position = AdjustPosition(newPosition);
        // Console.WriteLine($"X: {position.X}, Y: {position.Y}"); // For debugging purposes
    }

    #region OnPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string? name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    #endregion

    #region Accuracy Functions

    private ItemSize targetSize = new()
    {
        Width = 30,
        Height = 30
    };

    public ItemSize TargetSize
    {
        get => targetSize;
        set
        {
            targetSize = value;
            OnPropertyChanged(nameof(TargetSize));
        }
    }

    private int score = 0;

    public int Score
    {
        get => score;
        set
        {
            score = value;
            OnPropertyChanged(nameof(Score));
        }
    }

    private Vector targetPosition { get; set; } = new Vector(0,0);
    public Vector TargetPosition
    {
        get => targetPosition;
        set {
            targetPosition = value;
            OnPropertyChanged(nameof(TargetPosition));
        }
    }
    private bool isTargetSpawned { get; set; }
    public bool IsTargetSpawned
    {
        get => isTargetSpawned;
        set
        {
            isTargetSpawned = value;
            OnPropertyChanged(nameof(IsTargetSpawned));
            OnPropertyChanged(nameof(IsGameStarted));
            OnPropertyChanged(nameof(CheckTargetVisibility));
        }
    }

    public Visibility CheckTargetVisibility => IsTargetSpawned ? Visibility.Visible : Visibility.Hidden;
    public bool IsGameStarted => !IsTargetSpawned;

    private int _timeLeft;

    public int TimeLeft
    {
        get => _timeLeft;
        set
        {
            _timeLeft = value;
            OnPropertyChanged(nameof(TimeLeft));
        }
    }
    private readonly int _availableTime = 120;
    private readonly double _adjustment = 1;

    private Timer? Timer { get; set; }
    private Stopwatch? _stopwatch;
    private Task? _updateTimeTask;

    public void StartAccuracyGame()
    {
        Score = 0;
        IsTargetSpawned = false;
        SpawnTarget();

        Timer = new Timer((state) =>
        {
            StopAccuracyGame();
        }, null, _availableTime * 1000, Timeout.Infinite);
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
        this.PropertyChanged += HitCheck;
        _updateTimeTask = Task.Run(async () =>
        {
            while (_stopwatch.IsRunning)
            {
                await UpdateTime();
                await Task.Delay(100);
            }

            return Task.CompletedTask;
        });

    }

    public void StopAccuracyGame()
    {
        this.PropertyChanged -= HitCheck;

        if (Timer is not null)
            Timer.Dispose();
        if(_stopwatch is not null)
            _stopwatch.Stop();

        TimeLeft = 0;
        IsTargetSpawned = false;
        TargetPosition = new Vector(0, 0);
    }

    private async Task UpdateTime()
    {
        if(_stopwatch is not null)
            TimeLeft = _availableTime - ((int)_stopwatch.ElapsedMilliseconds / 1000);
    }

    private void HitCheck(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Position) && IsTargetSpawned)
            CheckIfTargetHit();
    }

    private void SpawnTarget()
    {
        int x = Random.Shared.Next(0, CanvasSize.Width);
        int y = Random.Shared.Next(0, CanvasSize.Height);

        TargetPosition = new Vector(x - TargetSize.Width / 2, y - TargetSize.Height / 2);
        IsTargetSpawned = true;
    }

    private void CheckIfTargetHit()
    {
        // Console.WriteLine("Target position: " +
        //                   $"X: {TargetPosition.X}, Y: {TargetPosition.Y}");
        // Console.WriteLine("Current position: " +
        //                   $"X: {_position.X}, Y: {_position.Y}");
        // For debugging purposes

        if(_position.X <= TargetPosition.X + TargetSize.Width * _adjustment &&
           _position.X >= TargetPosition.X - TargetSize.Width * _adjustment &&
           _position.Y <= TargetPosition.Y + TargetSize.Height * _adjustment &&
           _position.Y >= TargetPosition.Y - TargetSize.Height * _adjustment)
        {
            IsTargetSpawned = false;
            SpawnTarget();
            Score++;
        }
    }

    #endregion

    private double _speed = 5;
    public double Speed
    {
        get => _speed;
        set
        {
            _speed = value;
            Movement.Speed = _speed;
            OnPropertyChanged(nameof(Speed));
        }
    }
}