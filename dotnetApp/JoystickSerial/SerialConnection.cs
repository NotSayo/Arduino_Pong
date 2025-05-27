using System.IO.Ports;

namespace JoystickSerial;

public class SerialConnection : IDisposable
{
    private SerialPort? SerialPort { get; set; }


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

        SerialPort.ReadTimeout = 500;
    }

    public async Task Start(CancellationToken token)
    {
        if (SerialPort is null)
            throw new InvalidOperationException();
        if(SerialPort.IsOpen)
            SerialPort.Close();
        SerialPort.Open();

        while (!token.IsCancellationRequested)
        {
            try
            {
                string message = SerialPort.ReadLine();
                Console.WriteLine(message);
            }
            catch (TimeoutException e)
            {
                Console.WriteLine("Timeout");
            }
        }
        SerialPort.Close();

    }

    public void SetPortName(string portName) => SerialPort.PortName = portName;

    public string[] GetAllPorts() => SerialPort.GetPortNames();

    public void Dispose()
    {
        SerialPort.Close();
        SerialPort.Dispose();

    }
}