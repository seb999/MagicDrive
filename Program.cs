using System.Device.Gpio;
using MagicDrive.Misc;
using MagicDrive.MLClass;
using MMALSharp;
using MMALSharp.Common;
using MMALSharp.Components;
using MMALSharp.Handlers;
using MMALSharp.Native;
using MMALSharp.Ports;
using MMALSharp.Ports.Outputs;

namespace MagicDrive
{
    internal class Program
    {

        //to be defined
        private static string pictureFolderPath = @"home/Pictures";
        private static int STANDBY_LED = 23;
        private static int DRIVE_LED = 24;
        private static int START_BTN = 18;

        private static FileSystemWatcher? watcher;
        private static MMALCamera cam = MMALCamera.Instance;
        private static bool isAutoDrive = false;
        private static StepperMotorController? motor;
        private static LedController? standByLed;
        private static LedController? driveLed;
        private static SwitchController? startStopButton;
        private static FileWatcher? fileWatcher;

        private static void Main(string[] args)
        {
            // 0 - Uncomment when all part tested
            // motor = new StepperMotorController();
            // standByLed = new LedController(standbyLedGPIO);
            // magicDriveLed = new LedController(magicDriveLedGPIO);
            // startStopButton = new SwitchController(START_BTN,  OnButtonPinValueChanged);
            // fileWatcher = new FileWatcher(pictureFolderPath, OnFileCreated); 
            // magicDriveLed.Off();
            // standByLed.On();

            ///////////////////////////
            //      Debug      ////////
            ///////////////////////////
            //Test leds and motor and comment out
            standByLed = new LedController(STANDBY_LED);
            driveLed = new LedController(DRIVE_LED);
            motor = new StepperMotorController();

            while (1 == 1)
            {
                standByLed.Blink();
                standByLed.Blink();
                standByLed.Blink();
                driveLed.Blink();
                driveLed.Blink();
                Thread.Sleep(1000);
                motor.TurnLeft(100);
                Thread.Sleep(1000);
                motor.TurnRight(1000);
            }

            //Test event handler on new file
            //fileWatcher = new FileWatcher(pictureFolderPath, OnFileCreated); 

            //Test button active on / off
            //startStopButton = new SwitchController(START_BTN,  OnButtonPinValueChanged);

            //Test take picture
            //TakePicture();
        }

        private static void OnButtonPinValueChanged(object sender, PinValueChangedEventArgs e)
        {
            if (e.ChangeType == PinEventTypes.Falling)
            {
                if (!isAutoDrive)
                {
                     isAutoDrive = true;

                    ///////////////////////////
                    //      Debug      ////////
                    ///////////////////////////
                    standByLed.Blink();
                    standByLed.Blink();
                    standByLed.Blink();

                    // standByLed?.Off();
                    // driveLed?.On();
                    // fileWatcher.Start();
                    // TakePicture();
                }
                else
                {
                    isAutoDrive = false;

                    ///////////////////////////
                    //      Debug      ////////
                    ///////////////////////////
                    driveLed.Blink();
                    driveLed.Blink();

                    // standByLed?.On();
                    // driveLed?.Off();
                    // fileWatcher?.Stop();
                    // cam.Cleanup();
                }
            }
        }

        private static void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            fileWatcher?.Stop();

            ///////////////////////////
            //      Debug      ////////
            ///////////////////////////
            standByLed.Blink();
            standByLed.Blink();
            standByLed.Blink();
            driveLed.Blink();
            driveLed.Blink();

            //Comment out when debugging completed
            //ProcessImage(e.FullPath);

            CleanImageFolder();

            fileWatcher?.Start();
        }

        private static async void TakePicture()
        {
            using (var imgCaptureHandler = new ImageStreamCaptureHandler(pictureFolderPath, "jpg"))
            {
                var cts = new CancellationTokenSource(TimeSpan.FromHours(4));

                await cam.TakePictureTimeout(imgCaptureHandler, MMALEncoding.JPEG, MMALEncoding.I420, cts.Token);
            }

            // Only call when you no longer require the camera, i.e. on app shutdown.
            cam.Cleanup();
        }

        private static void ProcessImage(string imagePath)
        {

            var imageBytes = File.ReadAllBytes(imagePath);

            MCModel.ModelInput mlData = new MCModel.ModelInput()
            {
                ImageSource = imageBytes,
            };

            //Make a single prediction
            var predictionResult = MCModel.Predict(mlData);

            if (predictionResult.PredictedLabel == MyEnum.Correction.Left.ToString() && predictionResult.Score[0] > 60)
            {
                motor?.TurnRight(10); //steps and millisecond
            }

            if (predictionResult.PredictedLabel == MyEnum.Correction.Right.ToString() && predictionResult.Score[1] > 60)
            {
                motor?.TurnLeft(10); //steps and millisecond
            }
        }

        private static void CleanImageFolder()
        {
            //We delete all files after and wait a new picture from the camera
            DirectoryInfo di = new DirectoryInfo(pictureFolderPath);
            foreach (FileInfo file in di.EnumerateFiles())
            {
                file.Delete();
            }
        }
    }
}