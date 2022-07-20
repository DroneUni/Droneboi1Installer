using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;
using System.IO;
using File = System.IO.File;
using System.IO.Compression;
using Path = System.IO.Path;
using IWshRuntimeLibrary;

static class Installer
{
    private const string version = "0.44";
    public static async void Install(BackgroundWorker worker)
    {
        worker.ReportProgress(1);
        HttpClient http = new();
        HttpRequestMessage req = new();
        req.Method = HttpMethod.Get;
        req.RequestUri = new Uri("https://drive.google.com/uc?export=download&id=1Udg0ruB1_yi6gTlvVzA5N9Jxxqq9lljQ&confirm=t");
        HttpResponseMessage res = http.Send(req);
        if (!res.IsSuccessStatusCode)
        {
            worker.ReportProgress(2);
        }
        byte[] zipBytes = await res.Content.ReadAsByteArrayAsync();
        string tmpPath = Path.GetTempFileName();
        File.WriteAllBytes(tmpPath, zipBytes);
        string programPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + @"\Droneboi\" + version.Replace(".", "_");
        worker.ReportProgress(3);
        ZipFile.ExtractToDirectory(tmpPath, programPath);
        File.Delete(tmpPath);

        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var shell = new WshShell();
        var shortCutLinkFilePath = desktopPath+@"\Droneboi.lnk";
        var windowsApplicationShortcut = (IWshShortcut)shell.CreateShortcut(shortCutLinkFilePath);
        windowsApplicationShortcut.Description = "How to create short for application example";
        windowsApplicationShortcut.WorkingDirectory = desktopPath;
        windowsApplicationShortcut.TargetPath = programPath+@"\Droneboi.exe";
        windowsApplicationShortcut.Save();

        DirectoryInfo programDir = new(programPath);
        DirectoryInfo zipDir = programDir.EnumerateDirectories().ToArray()[0];
        zipDir.EnumerateFiles().ToList().ForEach(f => File.Move(f.FullName, $@"{programPath}\{f.Name}"));
        zipDir.EnumerateDirectories().ToList().ForEach(f => Directory.Move(f.FullName, $@"{programPath}\{f.Name}"));
        zipDir.Delete();
        worker.ReportProgress(100);

    }
}

namespace Droneboi1Installer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Start();
        }

        private void Start()
        {
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += (sender, args) =>
            {
                Installer.Install(sender as BackgroundWorker);
            };
            backgroundWorker.ProgressChanged += (_, args) =>
            {
                int code = args.ProgressPercentage;
                switch (code)
                {
                    case 1:
                        button.IsEnabled = false;
                        button.FontSize = 48;
                        button.Content = "Downloading";
                        break;
                    case 2:
                        button.Content = "Download failed";
                        button.IsEnabled = true;
                        break;
                    case 3:
                        button.Content = "Extracting";
                        break;
                    case 100:
                        button.FontSize = 72;
                        button.Content = "Installed";
                        break;
                }
            };
            button.Click += (_,_) =>
            {
                backgroundWorker.RunWorkerAsync();
            };
        }
        
    }
}

