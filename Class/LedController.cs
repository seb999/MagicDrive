using System.Device.Gpio;

public class LedController
{
    private readonly GpioController _controller;
    private readonly int _pinNumber;

    public LedController(int pin)
    {
        _pinNumber = pin;
        _controller = new GpioController();
        _controller.OpenPin(pin, PinMode.Output);
        _controller.Write(_pinNumber, PinValue.Low);
    }

    public void Blink()
    {
        _controller.Write(_pinNumber, PinValue.High);
        Thread.Sleep(300);
        _controller.Write(_pinNumber, PinValue.Low);
        Thread.Sleep(300);

        Dispose();
    }

    public void On()
    {
        _controller.Write(_pinNumber, PinValue.High);
    }

    public void Off()
    {
        _controller.Write(_pinNumber, PinValue.Low);
    }

    private void Dispose()
    {
        _controller.ClosePin(_pinNumber);
        _controller.Dispose();
    }
}