using System;
using System.Device.Gpio;
using System.Threading;
using MagicDrive.Misc;
using MagicDrive.ML;
using System.Diagnostics;
using System.Linq;
using MarginCoin.Misc;
using SocketIOClient;
using System.Text.Json;

class Program
{
    private static string bashPictureL = @"/home/sebastien/Git/MagicDrive/Script/takePicL.sh";
    private static string bashPictureR = @"/home/sebastien/Git/MagicDrive/Script/takePicR.sh";
    private static string bashPictureC = @"/home/sebastien/Git/MagicDrive/Script/takePicC.sh";
    private static string bashPictureVideo = @"/home/sebastien/Git/MagicDrive/Script/takeVideo.sh";

    
    private static bool isAutoDrive = false;
    private static LedController ledStandBy;
    private static LedController ledDriving;
    private static StepperMotorController motor;
    private static int offset = 0;

    static void Main(string[] args)
    {
        //leds
        ledStandBy = new LedController(2);
        ledDriving = new LedController(3);
        motor = new StepperMotorController();

        //keys
        var key1 = new SwitchController(21, OnBtnKey1Changed);
        var key2 = new SwitchController(20, OnBtnKey1Changed);
        var key3 = new SwitchController(16, OnBtnKey3Changed);

        //Joystick
        var right = new SwitchController(26, OnBtnRightChanged);
        var center = new SwitchController(13, OnBtnCenterChanged);
        var left = new SwitchController(5, OnBtnLeftChanged);

        var down = new SwitchController(6, OnBtnUpChanged);
        var up = new SwitchController(19, OnBtnDownChanged);


        ConnectToCamera();

        Standby();
    }

    private static void Standby()
    {
        while (true)
        {
            if (isAutoDrive)
            {
                ledStandBy.Off();
                ledDriving.Blink();
            }
            else
            {
                ledDriving.Off();
                ledStandBy.Blink();
            }
        }
    }

    private static void MotorOffset(MyEnum.offset dir)
    {
        switch (dir)
        {
            case MyEnum.offset.up:
                offset+=10;
                break;

            case MyEnum.offset.down:
                offset-=10;
                break;

            default:
                break;
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
                Console.WriteLine(result);
                if (isAutoDrive)
                {
                    PredictionResult predictionResult = JsonSerializer.Deserialize<PredictionResult>(result);

                    AdjustDirection(predictionResult.label, predictionResult.score);
                }
            });
            await client.ConnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error connecting to the Socket.IO server: " + ex.Message);
            // You can take further action here, such as logging or handling the error.
        }
    }

    static void OnBtnKey1Changed(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            if (!isAutoDrive)
            {
                isAutoDrive = true;
                ledDriving.Blink();
            }
            else
            {
                isAutoDrive = false;
            }

            Thread.Sleep(50);
        }
    }

    static void OnBtnKey3Changed(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            TakePicture(bashPictureVideo);
            ledDriving.Blink();
            Thread.Sleep(50);
        }

    }

    static void OnBtnUpChanged(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            MotorOffset(MyEnum.offset.up);
            motor.TurnLeft(600 + offset);
        }
    }

     static void OnBtnDownChanged(object sender, PinValueChangedEventArgs e)
    {
       if (e.ChangeType == PinEventTypes.Falling)
        {
            MotorOffset(MyEnum.offset.down);
            motor.TurnRight(600 + offset);
        }
    }

    static void OnBtnRightChanged(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            TakePicture(bashPictureR);
            ledDriving.Blink();
            Thread.Sleep(50);
        }
    }

    static void OnBtnCenterChanged(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            TakePicture(bashPictureC);
            ledDriving.Blink();
            Thread.Sleep(50);
        }
    }

    static void OnBtnLeftChanged(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            TakePicture(bashPictureL);
            ledDriving.Blink();
            Thread.Sleep(50);
        }
    }

    #region Camera and AI processing

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
                //motor.TurnRight(600);
                break;

            case "right":
                //motor.TurnLeft(600);
                break;

            case "center":
                break;
        }
    }

    static string? GetMostRecentImagePath(string folderPath)
    {
        // Get all image files in the folder
        string[] imageExtensions = { "*.jpg" };
        var imageFiles = imageExtensions.SelectMany(ext => Directory.GetFiles(folderPath, ext));

        if (imageFiles.Any())
        {
            // Get the most recent image file based on creation time
            string? mostRecentImagePath = imageFiles
                .Select(filePath => new FileInfo(filePath))
                .OrderByDescending(fileInfo => fileInfo.CreationTime)
                .FirstOrDefault()?.FullName;

            return mostRecentImagePath;
        }
        else
        {
            Console.WriteLine("No image files found in the folder.");
            return null;
        }
    }

    static void DeleteImage(string imagePath)
    {
        try
        {
            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
            else
            {
                Console.WriteLine("Image not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    #endregion
}
