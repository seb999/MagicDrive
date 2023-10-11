using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using MagicDrive.Misc;

public class SSD1306Controller
{
    private SpiDevice _spi;
    private GpioController _gpioController;
    private int _resetPin;
    private int _dcPin;

    private byte[] _buffer;

    public SSD1306Controller(SpiDevice spi, int resetPin, int dcPin)
    {
        _spi = spi;
        _resetPin = resetPin;
        _dcPin = dcPin;

        InitializeGpio();
        InitializeDisplay();
    }

    private void InitializeGpio()
    {
        _gpioController = new GpioController();

        // Set up the reset and data/command pins
        _gpioController.OpenPin(_resetPin, PinMode.Output);
        _gpioController.OpenPin(_dcPin, PinMode.Output);

        // Perform a hardware reset
        _gpioController.Write(_resetPin, PinValue.Low);
        Thread.Sleep(100);
        _gpioController.Write(_resetPin, PinValue.High);
    }

    private void InitializeDisplay()
    {
        // Initialize the SSD1306 display
        SendCommand(0xAE); // Display Off

        // Set the display to horizontal addressing mode
        SendCommand(0x20);
        SendCommand(0x00);

        // Set the display to normal scan direction
        SendCommand(0xC0);

        // Set the display to page addressing mode
        SendCommand(0x02);

        // Set the contrast level (adjust as needed)
        SendCommand(0x81);
        SendCommand(0xFF);

        // Set the segment re-map
        SendCommand(0xA1);

        // Set the COM output scan direction
        SendCommand(0xC8);

        // Set the display to inverse mode (0xA6 for normal mode)
        SendCommand(0xA7);

        SendCommand(0xAF); // Display On
    }

    public void Clear()
    {
        // Clear the display by filling it with zeros
        byte[] buffer = new byte[1025]; // 128x64 / 8
        buffer[0] = 0x40; // Data Mode
        _spi.Write(buffer);
    }

    public void DrawText(int x, int y, string text)
    {
        foreach (char character in text)
        {
            if (character < 32 || character > 126)
            {
                // Skip characters that are not in the font
                continue;
            }

            byte[] charData = Font5x7.Data[character - 32]; // Adjust for the ASCII offset

            for (int i = 0; i < 7; i++)
            {
                byte line = charData[i];
                for (int j = 0; j < 5; j++)
                {
                    if ((line & (1 << j)) != 0)
                    {
                        DrawPixel(x + j, y + i, true);
                    }
                }
            }

            // Move to the next character position
            x += 6; // 5 columns + 1 column spacing
        }
    }

    public void DrawHorizontalArrow(int x, int y, int length, MyEnum.ArrowDirection direction)
    {
        // Check the direction of the arrow
        bool isLeftArrow = direction == MyEnum.ArrowDirection.Left;

        // Calculate arrowhead size (2 pixels on each side)
        int arrowheadSize = 2;

        // Draw the arrow's body
        for (int i = 0; i < length; i++)
        {
            int xPos = isLeftArrow ? (x - i) : (x + i);
            DrawPixel(xPos, y, true);
        }

        // Draw the arrow's head
        for (int i = 0; i < arrowheadSize; i++)
        {
            int xPos = isLeftArrow ? (x - length - i) : (x + length + i);
            for (int j = -i; j <= i; j++)
            {
                int yPos = y + j;
                DrawPixel(xPos, yPos, true);
            }
        }
    }

    public void DrawPixel(int x, int y, bool color)
    {
        if (x < 0 || x >= 128 || y < 0 || y >= 64)
        {
            return; // Out of bounds
        }

        if (color)
        {
            _buffer[x + (y / 8) * 128] |= (byte)(1 << (y % 8));
        }
        else
        {
            _buffer[x + (y / 8) * 128] &= (byte)~(1 << (y % 8));
        }
    }

    public void SendDisplayData()
    {
        SendCommand(0x21); // Set Column Address
        SendCommand(0);    // Start Column
        SendCommand(127);  // End Column

        SendCommand(0x22); // Set Page Address
        SendCommand(0);    // Start Page
        SendCommand(7);    // End Page

        _gpioController.Write(_dcPin, PinValue.High); // Data Mode

        foreach (byte dataByte in _buffer)
        {
            _spi.WriteByte(dataByte);
        }
    }

    public void Show()
    {
        SendDisplayData();
    }

    private void SendCommand(byte command)
    {
        _gpioController.Write(_dcPin, PinValue.Low); // Command Mode
        _spi.WriteByte(command);
    }
}
