namespace JoystickSerial;

public enum SerialStatus
{
    NotInitialized,
    Initialized,
    PortOpen,
    PortClosed,
    DataReceived,
    Error
}