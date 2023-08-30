using System;
using System.Device.Gpio;
using System.Threading;
using MagicDrive.Misc;
using MagicDrive.ML;
using System.Diagnostics;
using System.Linq;

class Program
{
    private static string pictureFolderPath = @"/home/sebastien/Pictures/timeLapse";
    private static string bashPictureL = @"/home/sebastien/Git/MagicDrive/Script/takePicL.sh";
    private static string bashPictureR = @"/home/sebastien/Git/MagicDrive/Script/takePicR.sh";
    private static string bashPictureC = @"/home/sebastien/Git/MagicDrive/Script/takePicC.sh";
    private static string bashTimeLapse = @"/home/sebastien/Git/MagicDrive/Script/takeTimeLapse.sh";
    private static bool isActivated = false;
    private static LedController ledStandBy;
    private static LedController ledDriving;

    private static FileWatcher? fileWatcher;


    static Process extScript;

    static void Main(string[] args)
    {
        ledStandBy = new LedController(23);
        ledDriving = new LedController(24);
        var startStopButton = new SwitchController(18, OnBtnStartStopChanged);
        var rightButton = new SwitchController(25, OnBtnRightChanged);
        var centerButton = new SwitchController(8, OnBtnCenterChanged);
        var leftButton = new SwitchController(7, OnBtnLeftChanged);
        fileWatcher = new FileWatcher(pictureFolderPath, OnFileCreated);

        Standby();

    }

    private static void Standby()
    {
        while (true)
        {
            if (isActivated)
            {
                ledDriving.On();
                ledStandBy.Off();

               // var imagePath = GetMostRecentImagePath(pictureFolderPath);
               /// if (imagePath != null) ProcessImage(imagePath);

                Thread.Sleep(200);
            }
            else
            {
                ledStandBy.Blink();
                ledDriving.Off();
            }
        }
    }

    private static void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine("new image");
    }

    static void OnBtnStartStopChanged(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            if (!isActivated)
            {
                isActivated = true;
                ledStandBy.Off();
                ledDriving.On();

                TakeTimeLapse(bashTimeLapse);

                Console.WriteLine("time lapse running");
                Thread.Sleep(100);
            }
            else
            {
                isActivated = false;
                extScript.Close();
                ledDriving.Off();
                Thread.Sleep(100);
            }

        }
    }

    static void OnBtnRightChanged(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            TakePicture(bashPictureR);
        }
    }

    static void OnBtnCenterChanged(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            TakePicture(bashPictureC);
        }
    }

    static void OnBtnLeftChanged(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            TakePicture(bashPictureL);
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

    static void TakeTimeLapse1(string bashPath)
    {
        //find a way to start and kill the timelaspe
        Process extScript = new Process();
        extScript.StartInfo.UseShellExecute = true;
        extScript.StartInfo.FileName = bashPath;
        extScript.StartInfo.CreateNoWindow = true;
        extScript.Start();
        extScript.WaitForExit();
    }

    private static void TakeTimeLapse(string bashPath)
    {
        //find a way to start and kill the timelaspe
        extScript = new Process();
        extScript.StartInfo.UseShellExecute = true;
        extScript.StartInfo.FileName = bashPath;
        extScript.StartInfo.CreateNoWindow = true;
        extScript.Start();
        //extScript.WaitForExit();
    }

    private static void ProcessImage(string imagePath)
    {
        Console.WriteLine($"The image {imagePath} has been processed");

        // var imageBytes = File.ReadAllBytes(imagePath);

        // MLModel.ModelInput mlData = new MLModel.ModelInput()
        // {
        //     ImageSource = imageBytes,
        // };

        // //Make a single prediction
        // var predictionResult = MLModel.Predict(mlData);

        // Console.WriteLine($"Prediction : {predictionResult.PredictedLabel}");
        // if (predictionResult.PredictedLabel == MyEnum.Correction.Left.ToString() && predictionResult.Score[0] > 60)
        // {
        //     //motor?.TurnRight(10); //steps and millisecond
        // }

        // if (predictionResult.PredictedLabel == MyEnum.Correction.Right.ToString() && predictionResult.Score[1] > 60)
        // {
        //     //motor?.TurnLeft(10); //steps and millisecond
        // }
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

    private static void CleanImageFolder(string folderPath)
    {
        //We delete all files after and wait a new picture from the camera
        DirectoryInfo di = new DirectoryInfo(folderPath);
        foreach (FileInfo file in di.EnumerateFiles())
        {
            file.Delete();
        }
    }

    #endregion
}