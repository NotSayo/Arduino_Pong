using System.IO.Ports;

namespace JoystickSerial;

public class SerialConnection : IDisposable
{
    public delegate void DataReceivedEventHandler(JoyStickPosition position);
    public delegate void SerialStatusChangedEventHandler(SerialStatus status);
    public delegate void ErrorProcessingDataEventHandler(Exception e, string errorMessage = "");

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
    public event ErrorProcessingDataEventHandler? OnErrorProcessingData;


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
                JoyStickPosition position;
                try
                {
                    position = new JoyStickPosition(data);
                } catch(ArgumentException e)
                {
                    OnErrorProcessingData?.Invoke(e, "Invalid data format received from serial port.");
                    // Console.WriteLine($"Invalid data format: {e.Message}");
                    continue;
                } catch(IndexOutOfRangeException e)
                {
                    OnErrorProcessingData?.Invoke(e, "Data parsing error: Index out of range.");
                    // Console.WriteLine($"Data parsing error: {e.Message}");
                    continue;
                }
                OnDataReceived?.Invoke(position);
            }
            catch (TimeoutException e)
            {
                // Console.WriteLine("Timeout");
                OnErrorProcessingData?.Invoke(e, "Timeout while reading from serial port.");
            }
            catch(Exception e)
            {
                Status = SerialStatus.Error;
                OnErrorProcessingData?.Invoke(e, "Error reading from serial port.");
                // Console.WriteLine($"Error reading from serial port: {e.Message}"); // For debugging purposes
                break; // Exit the loop on error
            }
        }
        SerialPort.Close();
        Status = SerialStatus.PortClosed;
        // Console.WriteLine("Connection to Serial closed."); // For debugging purposes

    }

    // public void SetPortName(string portName) => SerialPort.PortName = portName;

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