using System.IO.Ports;

namespace JoystickSerial;

public class SerialConnection : IDisposable
{
    public delegate void DataReceivedEventHandler(JoyStickPosition position);
    public delegate void SerialStatusChangedEventHandler(SerialStatus status);

    private SerialPort? SerialPort { get; set; }

    private SerialStatus _status;
    public SerialStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            SerialStatusChanged?.Invoke(value);
        }
    }

    public event DataReceivedEventHandler? OnDataReceived;
    public event SerialStatusChangedEventHandler? SerialStatusChanged;


    public void InitializePort(
        string portName,
        int baudRate,
        int dataBits = 8,
        Parity parity = Parity.None,
        StopBits stopBits = StopBits.None,
        Handshake handshake = Handshake.None)
    {
        SerialPort = new SerialPort();
        SerialPort.PortName = portName;
        SerialPort.BaudRate = baudRate;
        SerialPort.Parity = parity;
        SerialPort.DataBits = dataBits;
        SerialPort.StopBits = stopBits;
        SerialPort.Handshake = handshake;

        SerialPort.ReadTimeout = 1000;
        Status = SerialStatus.Initialized;
    }

    public async Task Start(CancellationToken token)
    {
        if (SerialPort is null)
            throw new InvalidOperationException();
        if(SerialPort.IsOpen)
            SerialPort.Close();
        SerialPort.Open();
        Status = SerialStatus.PortOpen;

        while (!token.IsCancellationRequested)
        {
            try
            {
                string data = SerialPort.ReadLine();
                // Console.WriteLine(message);
                // string data = $"X:{500 + new Random().Next(-100, 100)},Y:{500 + new Random().Next(-100, 100)}";
                // Console.WriteLine(data); // For testing purposes, replace with SerialPort.ReadLine() in real use#
                JoyStickPosition position;
                try
                {
                     position = new JoyStickPosition(data);
                } catch(ArgumentException e)
                {
                    Console.WriteLine($"Invalid data format: {e.Message}");
                    continue;
                } catch(IndexOutOfRangeException e)
                {
                    Console.WriteLine($"Data parsing error: {e.Message}");
                    continue;
                }
                OnDataReceived?.Invoke(position);
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Timeout");
            }
            catch(Exception e)
            {
                Status = SerialStatus.Error;
                Console.WriteLine($"Error reading from serial port: {e.Message}");
                break; // Exit the loop on error
            }
        }
        SerialPort.Close();
        Status = SerialStatus.PortClosed;
        Console.WriteLine("Connection to Serial closed.");

    }

    public void SetPortName(string portName) => SerialPort.PortName = portName;

    public string[] GetAllPorts() => SerialPort.GetPortNames();

    public void Dispose()
    {
        if (SerialPort != null)
        {
            SerialPort.Close();
            SerialPort.Dispose();
        }
    }
}