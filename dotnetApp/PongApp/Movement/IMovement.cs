using JoystickSerial;

namespace PongApp.Movement;

public interface IMovement
{
    public double Speed { get; set; }
    public JoyStickPosition CalculatePosition(JoyStickPosition joyStickPosition);

}