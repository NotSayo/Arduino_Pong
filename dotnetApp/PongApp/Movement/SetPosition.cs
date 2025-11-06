using JoystickSerial;

namespace PongApp.Movement;

public class SetPosition : IMovement
{
    public double Speed { get; set; } = 3;
    private JoyStickPosition PlayedPosition { get; set; } = new JoyStickPosition(50, 50);
    public JoyStickPosition CalculatePosition(JoyStickPosition joyStickPosition)
    {
        int incrementX = (joyStickPosition.X - 500) / 100;
        if (incrementX > 6)
            incrementX = incrementX + 10;

        int incrementY = (joyStickPosition.Y - 500) / 100;
        if(incrementY > 6)
            incrementY = incrementY + 10;

        var newPosition = new JoyStickPosition(
            (int) (PlayedPosition.X + incrementX * Speed),
            (int) (PlayedPosition.Y + incrementY * Speed));

        if(PlayedPosition.X < 0)
            newPosition.X = 0;
        if(PlayedPosition.X > 1000)
            newPosition.X = 1000;
        if(PlayedPosition.Y < 0)
            newPosition.Y = 0;
        if (PlayedPosition.Y > 1000)
            newPosition.Y = 1000;

        PlayedPosition = newPosition;
        return newPosition;

    }
}