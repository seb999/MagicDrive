using System.Device.Gpio;
using System.Device.Spi;
using SkiaSharp;
using MagicDrive.Misc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
//https://github.com/rene-mt/esp8266-oled-sh1106/blob/master/SH1106.cpp

public class SSD1306Controller
{
    private SpiDevice _spi;
    private GpioController _gpioController;
    private int _resetPin;
    private int _dcPin;
    private byte[] _buffer;

    #region Constructor / Initialisation

    public SSD1306Controller(SpiDevice spi, int rst, int dcPin)
    {
        _spi = spi;
        _resetPin = rst;
        _dcPin = dcPin;
        _buffer = new byte[1024];
        _gpioController = new GpioController();
        _gpioController.OpenPin(_resetPin, PinMode.Output);
        _gpioController.OpenPin(_dcPin, PinMode.Output);

        Thread.Sleep(50);
        _gpioController.Write(_resetPin, PinValue.Low);
        Thread.Sleep(50);
        _gpioController.Write(_resetPin, PinValue.High);
        InitializeDisplay();
    }

    private void InitializeDisplay()
    {
        SendCommand(0xAE); // Display Off
        SendCommand(0x00); // Horizontal addressing mode
        SendCommand(0xA1); // Segment re-map
        SendCommand(0xC0); // COM Output Scan Direction
        SendCommand(0xDA); // Common pads hardware: alternative
        SendCommand(0x12);
        SendCommand(0x81); // Contrast Control
        SendCommand(0xCF);
        SendCommand(0xA4); // Entire Display On/Off
        SendCommand(0xA6); // Normal Display
        SendCommand(0xAF); // Display On
    }

    #endregion

    public void On()
    {
        SendCommand(0xAF); // Display On
    }

    public void Off()
    {
        SendCommand(0xAE); // Display Off
    }

    public void Clear()
    {
        Array.Clear(_buffer, 0, _buffer.Length);
    }

    public void Display()
    {
        SendCommand(0x21);// Set Column Address
        SendCommand(0x00);// Start Column
        SendCommand(0x7F); // End Column (127)

        SendCommand(0x22);// Set Page Address
        SendCommand(0x0);// Start Page
        SendCommand(0x7); // End Page

        SendCommand(0x40 | 0x00); //Start line

        for (byte page = 0; page < 64 / 8; page++)
        {
            for (byte col = 2; col < 130; col++)
            {
                SendCommand((byte)(0xB0 + page)); //Page start address + page
                SendCommand((byte)(col & 0xF));
                SendCommand((byte)(0x10 | (col >> 4))); //Set high column
                                                        // digitalWrite(myCS, HIGH);
                _gpioController.Write(_dcPin, PinValue.High);   // data mode
                                                                // digitalWrite(myCS, LOW);
                _spi.WriteByte(_buffer[col - 2 + page * 128]);

            }
        }
    }
   
    #region Base

    public void FlipScreen()
    {
        SendCommand(0xA0);              //SEGREMAP   //Rotate screen 180 deg
        SendCommand(0xDA);
        SendCommand(0x22);
        SendCommand(0xC0);            //COMSCANDEC  Rotate screen 180 Deg
    }

    private void SendCommand(byte command)
    {
        _gpioController.Write(_dcPin, PinValue.Low); // Command Mode
        _spi.WriteByte(command);
        _gpioController.Write(_dcPin, PinValue.High);
    }

    public void SetPixel(int x, int y, int color)
    {
        if (x >= 0 && x < 128 && y >= 0 && y < 64)
        {
            switch (color)
            {
                case 1:
                    _buffer[x + (y / 8) * 128] |= (byte)(1 << (y & 7));
                    break;

                case 0:
                    _buffer[x + (y / 8) * 128] &= (byte)~(1 << (y & 7));
                    break;
            }
        }
    }

    public void SetContrast(byte contrast)
    {
        SendCommand(0x81);
        SendCommand(contrast);
    }

    #endregion

    #region Drawing

    public void DrawText(int x, int y, string text, int color)
    {
        foreach (char character in text)
        {
            if (character < 32 || character > 126)
            {
                // Skip characters that are not in the font
                continue;
            }

            byte[] charData = Font.Font8x12[character - 32]; // Adjust for the ASCII offset

            for (int i = 0; i < 8; i++)
            {
                byte line = charData[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((line & (1 << j)) != 0)
                    {
                        SetPixel(x + j, y + i, color);
                    }
                }
            }

            // Move to the next character position
            x += 9; // 5 columns + 1 column spacing
        }
    }

    public void DrawRect(int x, int y, int width, int height, int color)
    {
        for (int i = x; i < x + width; i++)
        {
            SetPixel(i, y, 1);
            SetPixel(i, y + height, color);
        }
        for (int i = y; i < y + height; i++)
        {
            SetPixel(x, i, 1);
            SetPixel(x + width, i, color);
        }
    }

    public void DrawLineH(int x, int y, int length, int color)
    {
        for (int i = x; i < x + length; i++)
        {
            SetPixel(i, y, color);
        }
    }

    public void DrawLineV(int x, int y, int length, int color)
    {
        for (int i = y; i < y + length; i++)
        {
            SetPixel(x, i, color);
        }
    }

    public void FillRect(int x, int y, int width, int height, int color)
    {
        for (int i = x; i < x + width; i++)
        {
            for (int j = y; j < y + height; j++)
            {
                SetPixel(i, j, color);
            }
        }
    }

    public void DrawIcon(int x, int y, int width, int height, byte[] icon)
    {
        for (int i = 0; i < width * height / 8; i++)
        {
            byte charColumn = (byte)(icon[i]);
            for (int j = 0; j < 8; j++)
            {
                int targetX = i % width + x;
                int targetY = (i / width) * 8 + j + y;
                if ((charColumn & (1 << j)) == 0)
                {
                    // Assuming you have a SetPixel method to set individual pixels
                    SetPixel(targetX, targetY, 1);
                }
            }
        }
    }

    #endregion
}