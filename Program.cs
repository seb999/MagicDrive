using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Threading;
using MagicDrive.Misc;
using MagicDrive.ML;
using System.Diagnostics;
using System.Linq;
using MarginCoin.Misc;
using SocketIOClient;
using System.Text.Json;
//https://mischianti.org/images-to-byte-array-online-converter-cpp-arduino/  Ti convert image to byte array

//COMMANDS
//sudo sh Script/killBill.sh              ---kill MagicDrive processes
//sudo nano /etc/rc.local                 ---Edit boot file
//sudo reboot                             ---Reboot Raspberry
//dotnet publish --configuration Release  ---Publish

//SYSTEMD
//journalctl -u magicDriveStreaming.service
//sudo nano /lib/systemd/system/magicDriveStreaming.service
// sudo systemctl enable magicDriveStreaming.service 
//sudo systemctl daemon-reload
//sudo systemctl start magicDriveStreamingStreaming.service
//sudo systemctl stop magicDriveStreamingStreaming.service
// sudo systemctl disable magicDriveStreaming.service 

class Program
{
    private static string bashPictureL = @"/home/sebastien/Git/MagicDrive/Script/takePicL.sh";
    private static string bashPictureR = @"/home/sebastien/Git/MagicDrive/Script/takePicR.sh";
    private static string bashPictureC = @"/home/sebastien/Git/MagicDrive/Script/takePicC.sh";
    private static string bashPictureVideo = @"/home/sebastien/Git/MagicDrive/Script/takeVideo.sh";
    private static MyEnum.Mode mode = MyEnum.Mode.standby;
    private static bool isOledOff = false;
    private static StepperMotorController motor;
    private static SSD1306Controller oled;
    private static int offset = 0;
    private static bool booting = true; 
    private static List<double> AIscores = new List<double>();
    private static int AImaxScores = 5; // Maximum number of scores to display

    static void Main(string[] args)
    {
        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            // Your cleanup code here
            Console.WriteLine("Application is exiting...");
        };
        
        motor = new StepperMotorController();

        //keys
        var key1 = new SwitchController(21, OnBtnKey1Changed);
        var key2 = new SwitchController(20, OnBtnKey2Changed);
        var key3 = new SwitchController(16, OnBtnKey3Changed);

        //Joystick
        var right = new SwitchController(26, OnBtnRightChanged);
        var center = new SwitchController(13, OnBtnCenterChanged);
        var left = new SwitchController(5, OnBtnLeftChanged);

        var down = new SwitchController(6, OnBtnUpChanged);
        var up = new SwitchController(19, OnBtnDownChanged);

        // Create an SPI device
        var spiConnectionSettings = new SpiConnectionSettings(0, 0) { ClockFrequency = 1000000, Mode = 0b00 };
        var spiDevice = SpiDevice.Create(spiConnectionSettings);
        oled = new SSD1306Controller(spiDevice, 25, 24);

        oled.FlipScreen();

        //Drive Button
        oled.DrawLineH(106, 0, 40, 1);
        oled.DrawLineV(106, 0, 5, 1);
        oled.DrawText(110, 3, "DR", 1);

        //NA Button
        oled.DrawLineH(106, 25, 40, 1);
        oled.DrawLineV(106, 25, 5, 1);
        oled.DrawText(110, 28, "OL", 1);

        //TRAIN Button
        oled.DrawLineH(106, 51, 40, 1);
        oled.DrawLineV(106, 51, 5, 1);
        oled.DrawText(110, 55, "TR", 1);

        //Motor Offset
        oled.DrawText(0, 30, "-", 1);
        oled.DrawText(0, 20, offset.ToString(), 1);
        oled.DrawText(0, 10, "+", 1);

        //switch screen on and display
        oled.Display();

        booting = false;

        ConnectToCamera();

        Standby();
    }
    

    private static void Standby()
    {
        while (true)
        {
            switch (mode)
            {
                case MyEnum.Mode.standby:
                    oled.FillRect(38, 0, 50, 12, 0);
                    oled.FillRect(20, 0, 68, 12, 1);
                    oled.DrawText(23, 2, "STANDBY", 0);
                    oled.Display();
                    Thread.Sleep(800);
                    oled.FillRect(20, 0, 68, 12, 0);
                    oled.Display();
                    Thread.Sleep(500);
                    break;

                case MyEnum.Mode.drive:
                    oled.FillRect(20, 0, 68, 12, 0);
                    oled.FillRect(35, 0, 50, 12, 1);
                    oled.DrawText(38, 2, "DRIVE", 0);
                    oled.Display();
                    break;

                case MyEnum.Mode.train:
                    oled.FillRect(20, 0, 68, 12, 0);
                    oled.FillRect(25, 0, 50, 12, 1);
                    oled.DrawText(28, 2, "TRAIN", 0);
                    oled.Display();
                    break;
            }
        }
    }

    private async static void ConnectToCamera()
    {
        try
        {
            var client = new SocketIO("http://127.0.0.1:5000");
            client.On("message", response =>
            {
                string result = response.GetValue<string>();
                if (mode == MyEnum.Mode.drive)
                {
                    PredictionResult predictionResult = JsonSerializer.Deserialize<PredictionResult>(result);

                    AddScore(Math.Round(predictionResult.score*100, 3));

                    AdjustDirection(predictionResult.label, predictionResult.score);
                }
            });
            await client.ConnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error connecting to the Socket.IO server: " + ex.Message);
        }
    }

    #region Joystick / switch

    static void OnBtnKey1Changed(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            if (mode == MyEnum.Mode.standby || mode == MyEnum.Mode.train)
            {
                mode = MyEnum.Mode.drive;
            }
            else
            {
                oled.FillRect(34, 14, 60, 70, 0);
                mode = MyEnum.Mode.standby;
            }
            Thread.Sleep(50);
        }
    }

    static void OnBtnKey2Changed(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            if (!isOledOff)
            {
                isOledOff = true;
                oled.Off();
                return;
            }
            else
            {
                isOledOff = false;
                oled.On();
                return;
            }
        }
    }

    static void OnBtnKey3Changed(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            if (mode != MyEnum.Mode.train)
            {
                mode = MyEnum.Mode.train;
                return;
            }
            else
            {
                mode = MyEnum.Mode.standby;
                return;
            }
        }
    }

    static void OnBtnUpChanged(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            if (booting) return;
            if (mode != MyEnum.Mode.train)
            {
                MotorOffset(MyEnum.Offset.up);
                return;
            }
            else
            {
                oled.FillRect(30, 30, 35, 35, 0);
                oled.DrawIcon(30, 30, 32, 32, Icon.camera32x32);
                TakePicture(bashPictureVideo);
                Thread.Sleep(50);
                oled.FillRect(30, 30, 35, 35, 0);
                return;
            }
        }
    }

    static void OnBtnDownChanged(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            if (booting) return;
            if (mode != MyEnum.Mode.train)
            {
                MotorOffset(MyEnum.Offset.down);
                return;
            }
            else
            {
                oled.FillRect(30, 30, 35, 35, 0);
                oled.DrawIcon(30, 30, 32, 32, Icon.camera32x32);
                TakePicture(bashPictureVideo);
                Thread.Sleep(50);
                oled.FillRect(30, 30, 35, 35, 0);
                return;
            }
        }
    }

    static void OnBtnRightChanged(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            if (booting) return;
            if (mode == MyEnum.Mode.train)
            {
                oled.FillRect(30, 30, 35, 35, 0);
                oled.DrawIcon(30, 30, 32, 32, Icon.camera32x32);
                TakePicture(bashPictureR);
                Thread.Sleep(50);
                oled.FillRect(30, 30, 35, 35, 0);
            }
            else
            {
                motor.TurnRight(200 + offset * 10);
            }
        }
    }

    static void OnBtnLeftChanged(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            if (booting) return;
            if (mode == MyEnum.Mode.train)
            {
                oled.FillRect(30, 30, 35, 35, 0);
                oled.DrawIcon(30, 30, 32, 32, Icon.camera32x32);
                TakePicture(bashPictureL);
                Thread.Sleep(50);
                oled.FillRect(30, 30, 35, 35, 0);
            }
            else
            {
                motor.TurnLeft(200 + offset * 10);
            }
        }
    }

    static void OnBtnCenterChanged(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            if (booting) return;
            if (mode == MyEnum.Mode.train)
            {
                oled.FillRect(30, 30, 35, 35, 0);
                oled.DrawIcon(30, 30, 32, 32, Icon.camera32x32);
                TakePicture(bashPictureC);
                Thread.Sleep(50);
                oled.FillRect(30, 30, 35, 35, 0);
            }
        }
    }

    #endregion

    #region Helper

    static void TakePicture(string bashPath)
    {
        Process extScript = new Process();
        extScript.StartInfo.UseShellExecute = true;
        extScript.StartInfo.FileName = bashPath;
        extScript.StartInfo.CreateNoWindow = true;
        extScript.Start();
        extScript.WaitForExit();
    }

    private static void AdjustDirection(string label, float score)
    {
        switch (label)
        {
            case "left":
                oled.FillRect(1, 45, 32, 32, 0);
                oled.DrawIcon(1, 45, 32, 32, Icon.right32x32);
                motor.TurnRight(50 + offset * 10);
                break;

            case "right":
                oled.FillRect(1, 45, 32, 32, 0);
                oled.DrawIcon(1, 45, 32, 32, Icon.left32x32);
                 motor.TurnLeft(50 + offset * 10);
                break;

            case "center":
                oled.FillRect(1, 45, 32, 32, 0);
                oled.DrawIcon(1, 45, 32, 32, Icon.center32x32);
                break;
        }
    }

    private static void MotorOffset(MyEnum.Offset dir)
    {
        switch (dir)
        {
            case MyEnum.Offset.up:
                oled.DrawText(0, 20, offset.ToString(), 0);
                offset += 1;
                oled.DrawText(0, 20, offset.ToString(), 1);
                break;

            case MyEnum.Offset.down:
                oled.DrawText(0, 20, offset.ToString(), 0);
                offset -= 1;
                oled.DrawText(0, 20, offset.ToString(), 1);
                break;

            default:
                break;
        }
    }

    public static void AddScore(double newScore)
    {
        AIscores.Insert(0, newScore); // Add the new score at the beginning of the list

        if (AIscores.Count > AImaxScores)
        {
            AIscores.RemoveAt(AImaxScores); // Remove the last score if there are more than maxScores
        }

        DisplayScores();
    }

    private static void DisplayScores()
    {
        oled.FillRect(34, 14, 60, 75, 0);
        for (int i = 0; i < AIscores.Count; i++)
        {
            oled.DrawText(34, 15 + (10 * i), AIscores[i].ToString(), 1);
        }
    }
    
    #endregion
}
