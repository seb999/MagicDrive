namespace MagicDrive.Misc
{
    public class FileWatcher
    {
        private static FileSystemWatcher? watcher;
        public FileWatcher(string pictureFolder, FileSystemEventHandler OnFileCreated)
        {
            watcher = new FileSystemWatcher();
            watcher.Path = pictureFolder;
            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;
            watcher.Created += OnFileCreated;
        }


        public void Stop()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = false;
            }
        }

        public void Start()
        {
            if (watcher != null)
            {
                watcher.EnableRaisingEvents = true;
            }
        }

        public static void Dispose()
        {
            watcher?.Dispose();
            Console.WriteLine("Watcher disposed.");
        }
    }
}