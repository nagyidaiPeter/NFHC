using NfhcLauncher.Patching;
using NfhcModel;
using NfhcModel.Logger;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace NfhcLauncher.ViewModels
{
    public class MainViewModel : IDisposable, INotifyPropertyChanged
    {
        public static string Version => Assembly.GetAssembly(typeof(Extensions)).GetName().Version.ToString();
        public static MainViewModel Instance { get; private set; }

        public const string RELEASE_PHASE = "ALPHA";
        private NfhcEntryPatch NfhcEntryPatch;
        private string installPath = @"K:\SteamLibrary\steamapps\common\Neighbours back From Hell";
        private Process serverProcess;
        private Process gameProcess;
        private bool isEmbedded;
        string NfhcAppData;

        public string NfhPath
        {
            get => installPath;
            set
            {
                value = Path.GetFullPath(value);
                installPath = value;
                File.WriteAllText(Path.Combine(NfhcAppData, "gamePath.txt"), installPath);
                OnPropertyChanged();
            }
        }


        public RelayCommand Patch { get; }
               

        public MainViewModel()
        {

            NfhcAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nfhc");
            Directory.CreateDirectory(NfhcAppData);
            File.WriteAllText(Path.Combine(NfhcAppData, "launcherpath.txt"), Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            if (File.Exists(Path.Combine(NfhcAppData, "gamePath.txt")))
            {
                NfhPath = File.ReadAllText(Path.Combine(NfhcAppData, "gamePath.txt"));
            }            

            Patch = new RelayCommand(o => 
            {
                // TODO: The launcher should override FileRead win32 API for the Subnautica process to give it the modified Assembly-CSharp from memory 
                string bootloaderName = "NfhcBootloader.dll";
                try
                {
                    File.Copy(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), bootloaderName), Path.Combine(installPath, "Neighbours back From Hell_Data", "Managed", bootloaderName), true);
                }
                catch (IOException ex)
                {
                    Log.Error(ex, "Unable to move bootloader dll to Managed folder. Still attempting to launch because it might exist from previous runs.");
                }

                if (NfhcEntryPatch == null)
                {
                    NfhcEntryPatch = new NfhcEntryPatch(NfhPath);
                }

                NfhcEntryPatch.Remove(); // Remove any previous instances first.
                NfhcEntryPatch.Apply();
            });
        }


        public void Dispose()
        {
            NfhcEntryPatch.Remove();
        }

        public string SetTargetedNfhPath(string path)
        {
            if (!Directory.Exists(path))
            {
                return null;
            }
            NfhPath = path;

            File.WriteAllText("path.txt", path);
            if (NfhcEntryPatch?.IsApplied == true)
            {
                NfhcEntryPatch.Remove();
            }
            NfhcEntryPatch = new NfhcEntryPatch(path);

            if (Path.GetFullPath(path).StartsWith(AppHelper.ProgramFileDirectory, StringComparison.OrdinalIgnoreCase))
            {
                AppHelper.RestartAsAdmin();
            }

            return path;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
