using System.Device.Gpio;

public class SwitchController
{
    private readonly GpioController _controller;
    private readonly int _pinNumber;
   // public PinChangeEventHandler SwitchCallbackMethod();

    public SwitchController(int pin, PinChangeEventHandler onPinValueChanged)
    {
        _pinNumber = pin;
        _controller = new GpioController();
        _controller.OpenPin(pin, PinMode.Input);
        _controller.Write(_pinNumber, PinValue.High);

        _controller.RegisterCallbackForPinValueChangedEvent(
                pin,
                PinEventTypes.Falling | PinEventTypes.Rising, // Detect both rising and falling edge events
                onPinValueChanged);
    }

    public bool IsHigh()
    {
        if(_controller.Read(_pinNumber) == PinValue.High)
        {
            return true;
        }

        return false;
    }

    private void Dispose()
    {
        _controller.ClosePin(_pinNumber);
        _controller.Dispose();
    }
}