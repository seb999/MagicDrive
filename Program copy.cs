// using System.Device.Gpio;
// using MagicDrive.Misc;
// using MagicDrive.MLClass;
// using MMALSharp;
// using MMALSharp.Common;
// using MMALSharp.Handlers;

// namespace MagicDrive
// {
//     internal class Program
//     {

//         // PIN / Variables
//         private static string pictureFolderPath = @"home/Pictures";
//         private static bool isAutoDrive = false;
//         private static bool isTrainModel = false;
//         private static int LED_STANDBY = 23;
//         private static int LED_DRIVING = 24;
//         private static int BTN1 = 18;
//         private static int BTN2 = 17;
//         private static int BTN3 = 27;
//         private static int BTN4 = 22;

//         // Led
//         private static LedController? ledStandBy;
//         private static LedController? ledDriving;

//         // Switch
//         private static SwitchController? startStopButton;
//         private static SwitchController? leftButton;
//         private static SwitchController? rightButton;
//         private static SwitchController? centerButton;

//         //other
//         private static MMALCamera cam = MMALCamera.Instance;
//         private static StepperMotorController? motor;
//         private static FileWatcher? fileWatcher;

//         private static void Main(string[] args)
//         {
//             motor = new StepperMotorController();
//             ledStandBy = new LedController(LED_STANDBY);
//             ledDriving = new LedController(LED_DRIVING);
//             startStopButton = new SwitchController(BTN1, OnBtnStartStopChanged);
//             leftButton = new SwitchController(BTN2, OnBtnLeftChanged);
//             centerButton = new SwitchController(BTN3, OnBtnCenterChanged);
//             rightButton = new SwitchController(BTN4, OnBtnRightChanged);

//             fileWatcher = new FileWatcher(pictureFolderPath, OnFileCreated);
//             ledDriving.Off();
//             ledStandBy.On();

//             Debug();

//             //2 - Train model
//             // isTrainModel = true;
//         }

//         #region Degugging

//         private static void Debug()
//         {
//             //Test leds and motor and comment out
//             ledStandBy = new LedController(LED_STANDBY);
//             ledDriving = new LedController(LED_DRIVING);
//             motor = new StepperMotorController();

//             while (1 == 1)
//             {
//                 ledStandBy.Blink();
//                 ledStandBy.Blink();
//                 ledStandBy.Blink();
//                 ledDriving.Blink();
//                 ledDriving.Blink();
//                 Thread.Sleep(1000);
//                 motor.TurnLeft(100);
//                 Thread.Sleep(1000);
//                 motor.TurnRight(1000);
//             }

//             //Test take picture
//             //TakePicture();

//             //Collect picture to train model
//             // isCollectPicture = true;
//         }

//         #endregion

//         #region button event handler

//         private static void OnBtnLeftChanged(object sender, PinValueChangedEventArgs e)
//         {
//             if (e.ChangeType == PinEventTypes.Falling)
//             {
//                 if (isTrainModel)
//                 {
//                     TakePicture("Left");
//                 }
//                 else
//                 {
//                     //Stop AI and drive to left
//                     isAutoDrive = false;
//                     ledStandBy?.On();
//                     ledDriving?.Off();
//                     fileWatcher?.Stop();
//                     motor.TurnLeft(50);
//                 }
//             }
//         }

//         private static void OnBtnRightChanged(object sender, PinValueChangedEventArgs e)
//         {
//             if (isTrainModel)
//                 {
//                     TakePicture("Right");
//                 }
//                 else
//                 {
//                     //Stop AI and drive to left
//                     isAutoDrive = false;
//                     ledStandBy?.On();
//                     ledDriving?.Off();
//                     fileWatcher?.Stop();
//                     motor.TurnRight(50);
//                 }
//         }

//         private static void OnBtnCenterChanged(object sender, PinValueChangedEventArgs e)
//         {
//            if (isTrainModel)
//                 {
//                     TakePicture("Center");
//                 }
//                 else
//                 {
//                     //Stop AI and drive to left
//                     isAutoDrive = true;
//                     ledStandBy?.Off();
//                     ledDriving?.On();
//                     fileWatcher?.Start();
//                 }
//         }
//         private static void OnBtnStartStopChanged(object sender, PinValueChangedEventArgs e)
//         {
//             if (e.ChangeType == PinEventTypes.Falling)
//             {
//                 if (!isAutoDrive)
//                 {
//                     isAutoDrive = true;

//                     ///////////////////////////
//                     //      Debug      ////////
//                     ///////////////////////////
//                     ledStandBy.Blink();
//                     ledStandBy.Blink();
//                     ledStandBy.Blink();

//                     // standByLed?.Off();
//                     // driveLed?.On();
//                     // fileWatcher.Start();
//                     // TakePictureTimeout();
//                 }
//                 else
//                 {
//                     isAutoDrive = false;

//                     ///////////////////////////
//                     //      Debug      ////////
//                     ///////////////////////////
//                     ledDriving.Blink();
//                     ledDriving.Blink();

//                     // standByLed?.On();
//                     // driveLed?.Off();
//                     // fileWatcher?.Stop();
//                     // cam.Cleanup();
//                 }
//             }
//         }

//         #endregion

//         #region files event handler

//         private static void OnFileCreated(object sender, FileSystemEventArgs e)
//         {
//             if (!isTrainModel)
//             {
//                 fileWatcher?.Stop();

//                 ///////////////////////////
//                 //      Debug      ////////
//                 ///////////////////////////
//                 ledStandBy.Blink();
//                 ledStandBy.Blink();
//                 ledStandBy.Blink();
//                 ledDriving.Blink();
//                 ledDriving.Blink();

//                 //Comment out when debugging completed
//                 //ProcessImage(e.FullPath);

//                 CleanImageFolder();

//                 fileWatcher?.Start();
//             }
//         }

//         #endregion

//         #region Camera and AI processing

//         private static async void TakePicturTimeout()
//         {
//             using (var imgCaptureHandler = new ImageStreamCaptureHandler(pictureFolderPath, "jpg"))
//             {
//                 var cts = new CancellationTokenSource(TimeSpan.FromHours(4));

//                 await cam.TakePictureTimeout(imgCaptureHandler, MMALEncoding.JPEG, MMALEncoding.I420, cts.Token);
//             }

//             // Only call when you no longer require the camera, i.e. on app shutdown.
//             cam.Cleanup();
//         }

//         private static async void TakePicture(string folder)
//         {
//             using (var imgCaptureHandler = new ImageStreamCaptureHandler(pictureFolderPath, "jpg"))
//             {
//                 await cam.TakePicture(imgCaptureHandler, MMALEncoding.JPEG, MMALEncoding.I420);
//                 var source = imgCaptureHandler.GetFilepath();
//                 var destination = $"{pictureFolderPath}/{folder}";
//                 File.Move(source, destination);
//             }
//             // Only call when you no longer require the camera, i.e. on app shutdown.
//             cam.Cleanup();
//         }

//         private static void ProcessImage(string imagePath)
//         {

//             var imageBytes = File.ReadAllBytes(imagePath);

//             MCModel.ModelInput mlData = new MCModel.ModelInput()
//             {
//                 ImageSource = imageBytes,
//             };

//             //Make a single prediction
//             var predictionResult = MCModel.Predict(mlData);

//             if (predictionResult.PredictedLabel == MyEnum.Correction.Left.ToString() && predictionResult.Score[0] > 60)
//             {
//                 motor?.TurnRight(10); //steps and millisecond
//             }

//             if (predictionResult.PredictedLabel == MyEnum.Correction.Right.ToString() && predictionResult.Score[1] > 60)
//             {
//                 motor?.TurnLeft(10); //steps and millisecond
//             }
//         }

//         private static void CleanImageFolder()
//         {
//             //We delete all files after and wait a new picture from the camera
//             DirectoryInfo di = new DirectoryInfo(pictureFolderPath);
//             foreach (FileInfo file in di.EnumerateFiles())
//             {
//                 file.Delete();
//             }
//         }

//         #endregion
//     }
// }