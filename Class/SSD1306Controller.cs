using System.Device.Gpio;
using System.Device.I2c;

public class SSD1306Controller
{
    private const int ResetPin = 17;
    private const int Address = 0x3C;

    private readonly I2cDevice _i2cDevice;
    private readonly GpioController _gpioController;

    public SSD1306Controller()
    {
        _i2cDevice = I2cDevice.Create(new I2cConnectionSettings(1, Address));
        _gpioController = new GpioController(PinNumberingScheme.Board);

        // Set up the reset pin
        _gpioController.OpenPin(ResetPin, PinMode.Output);
        _gpioController.Write(ResetPin, PinValue.Low);
        System.Threading.Thread.Sleep(50);
        _gpioController.Write(ResetPin, PinValue.High);
        System.Threading.Thread.Sleep(50);

        // Initialize the display
        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0xAE); // Display off

        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0xD5); // Set display clock divide ratio/oscillator frequency
        _i2cDevice.WriteByte(0xF0); // Set divide ratio

        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0xA8); // Set multiplex ratio
        _i2cDevice.WriteByte(0x3F); // 1/64 duty

        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0xD3); // Set display offset
        _i2cDevice.WriteByte(0x00); // No offset

        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0x40); // Set start line

        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0x8D); // Charge pump setting
        _i2cDevice.WriteByte(0x14); // Enable charge pump

        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0x20); // Set memory addressing mode
        _i2cDevice.WriteByte(0x00); // Horizontal addressing mode

        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0xA1); // Set segment re-map

        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0xC8); // Set COM output scan direction

        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0xDA); // Set COM pins hardware configuration
        _i2cDevice.WriteByte(0x12); // Alternative COM pin configuration

        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0x81); // Set contrast control
        _i2cDevice.WriteByte(0xCF); // Set contrast

        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0xD9); // Set pre-charge period
        _i2cDevice.WriteByte(0xF1); // Set pre-charge period
        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0xA4); // Display all on/resume to RAM content

        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0xA6); // Normal display mode (A6), Inverse display mode (A7)

        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0xAF); // Display on
    }

    public void ClearDisplay()
    {
        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0x21); // Set column address
        _i2cDevice.WriteByte(0x00); // Column start address
        _i2cDevice.WriteByte(0x7F); // Column end address

        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0x22); // Set page address
        _i2cDevice.WriteByte(0x00); // Page start address
        _i2cDevice.WriteByte(0x07); // Page end address

        for (int i = 0; i < 1024; i++)
        {
            _i2cDevice.WriteByte(0x00); // Write zeros to all display memory
        }
    }

    public void DrawPixel(int x, int y, bool color)
    {
        if (x < 0 || x >= 128 || y < 0 || y >= 64)
        {
            return; // Don't draw outside the bounds of the display
        }

        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0x21); // Set column address
        _i2cDevice.WriteByte((byte)x); // Column start address
        _i2cDevice.WriteByte((byte)127); // Column end address

        _i2cDevice.WriteByte(0x00); // Command byte
        _i2cDevice.WriteByte(0x22); // Set page address
        _i2cDevice.WriteByte((byte)(y / 8)); // Page start address
        _i2cDevice.WriteByte((byte)7); // Page end address

        byte pixelMask = (byte)(1 << (y % 8));
        byte pixelValue = (byte)(_i2cDevice.ReadByte() | pixelMask);
        _i2cDevice.WriteByte(pixelValue); // Write the pixel data to the display memory
    }

    public void WriteChar(char character, Font font, int x, int y, bool color)
    {
        byte[] charData = font.GetCharData(character);
        int charWidth = font.CharWidth;
        int charHeight = font.CharHeight;

        for (int row = 0; row < charHeight; row++)
        {
            byte rowData = charData[row];

            for (int col = 0; col < charWidth; col++)
            {
                if ((rowData & 0x80) != 0)
                {
                    DrawPixel(x + col, y + row, color);
                }

                rowData <<= 1;
            }
        }
    }
}

public class Font
{
    private byte[][] _charData;
    private int _charWidth;
    private int _charHeight;

    public Font(byte[][] charData, int charWidth, int charHeight)
    {
        _charData = charData;
        _charWidth = charWidth;
        _charHeight = charHeight;
    }

    public byte[] GetCharData(char character)
    {
        int index = (int)character - 32;

        if (index < 0 || index >= _charData.Length)
        {
            return null;
        }

        return _charData[index];
    }

    public int CharWidth
    {
        get { return _charWidth; }
    }

    public int CharHeight
    {
        get { return _charHeight; }
    }
}
