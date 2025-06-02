using System.IO.Ports;
using JoystickSerial;

namespace PongConsole;

public class Program
{
    public static void Main(string[] args)
    {

        SerialConnection serial = new SerialConnection();

        var ports = serial.GetAllPorts();

        foreach (var port in ports)
            Console.WriteLine(port);

        while (true)
        {
            if (ports.Length == 0)
            {
                Console.WriteLine("No available Ports detected!");
                Console.WriteLine("Connect one and try again");
                Console.WriteLine("Click 'Enter' to refresh ...");
                Console.ReadLine();
                continue;
            }
            Console.WriteLine("Select an available port:");
            for (int i = 0; i < ports.Length; i++)
                Console.WriteLine($"{i}. {ports[i]}");

            Console.Write("Enter Port: ");
            var input = Console.ReadLine();
            Console.WriteLine();

            if (!int.TryParse(input, out var index))
                continue;
            if (index > ports.Length - 1 || index < 0)
                continue;
            serial.InitializePort(ports[index], 9600, 8, Parity.None, StopBits.One);
            break;
        }


        CancellationTokenSource tokenSource = new CancellationTokenSource();
        Task serialTask = Task.Run(() => serial.Start(tokenSource.Token), tokenSource.Token);
        Console.WriteLine("Type 'q' to exit:");
        while (!serialTask.IsCompleted)
        {
            var entry = Console.ReadKey();
            if(entry.KeyChar == 'q')
                tokenSource.Cancel();
        }


    }
}
