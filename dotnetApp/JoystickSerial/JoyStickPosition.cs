namespace JoystickSerial;

public class JoyStickPosition
{
    public int X { get; set; }
    public int Y { get; set; }

    public JoyStickPosition(int x, int y)
    {
        X = x;
        Y = y;
    }
    public JoyStickPosition(string data)
    {
        var cords = data.Trim().Split(',');
        if (cords.Length != 2)
            throw new ArgumentException("Data must contain two coordinates separated by a comma.");
        if (!int.TryParse(cords[0].Split(':')[1], out int x) || !int.TryParse(cords[1].Split(':')[1], out int y))
            throw new ArgumentException("Data must be in the format 'X:<value>,Y:<value>'.");
        X = x;
        Y = y;
    }
}