using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfhcModel.Helpers
{
    class NfhcAppData
    {
        private static NfhcAppData instance;

        private NfhcAppData()
        {
            Directory.CreateDirectory(RootDir);
        }

        public static NfhcAppData Instance => instance ?? (instance = new NfhcAppData());

        private string RootDir { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nfhc");

        /// <summary>
        ///     Gets the launcher path from the environment variable.
        ///     Set by Nitrox.Bootloader.Main.
        /// </summary>
        public string LauncherPath => Environment.GetEnvironmentVariable("NFHC_LAUNCHER_PATH");

        public string AssetsPath => Path.Combine(LauncherPath, "AssetBundles");

        public static NfhcAppData Load()
        {
            return new NfhcAppData();
        }
    }
}
