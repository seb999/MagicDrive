using System.Device.Gpio;

public class LedController
{
    private GpioController _controller{get;set;}
    private readonly int pin;

    private static SSD1306Controller oled;

    public LedController(int pinNumber)
    {
        this.pin = pinNumber;
        _controller = new GpioController();
        _controller.OpenPin(pin, PinMode.Output);
        _controller.Write(this.pin, PinValue.Low);
    }

    public void Blink()
    {
        _controller.Write(pin, PinValue.High);
        Thread.Sleep(50);
        _controller.Write(pin, PinValue.Low);
        Thread.Sleep(1500);
    }

    public void On()
    {
        _controller.Write(pin, PinValue.High);
    }

    public void Off()
    {
        _controller.Write(pin, PinValue.Low);
    }

    private void Dispose()
    {
        _controller.ClosePin(pin);
        _controller.Dispose();
    }
}