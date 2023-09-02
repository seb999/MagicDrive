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
    private static bool isProcessingPic = false;

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
        string? lastImage = GetMostRecentImagePath(pictureFolderPath);
        if(lastImage == null) return;
        Console.WriteLine(lastImage);
        if(!isProcessingPic) 
        {
            ProcessImage(lastImage);
        }
        else
        {
            return;
        }
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
            ledDriving.Blink();
            Thread.Sleep(50);

             MLConverterOnnx ttt = new MLConverterOnnx();
        ttt.ConvertModel();
        }
    }

    static void OnBtnCenterChanged(object sender, PinValueChangedEventArgs e)
    {
        if (e.ChangeType == PinEventTypes.Falling)
        {
            TakePicture(bashPictureC);
            ledDriving.Blink();
            Thread.Sleep(50);        }
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

        isProcessingPic = true;
      
        //Call Python service to inference model

        DeleteImage(imagePath);
        isProcessingPic = false;
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