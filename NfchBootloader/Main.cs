using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NfhcBootloader
{
    public static class Main
    {
        private static readonly Lazy<string> nfhcLauncherDir = new Lazy<string>(() =>
        {
            // Get path from command args.
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i].Equals("-Nfhc", StringComparison.OrdinalIgnoreCase) && Directory.Exists(args[i + 1]))
                {
                    return Path.GetFullPath(args[i + 1]);
                }
            }

            // Get path from AppData file.
            string NfhcAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Nfhc");
            if (!Directory.Exists(NfhcAppData))
            {
                return null;
            }
            string nfhcLauncherPathFile = Path.Combine(NfhcAppData, "launcherpath.txt");
            if (!File.Exists(nfhcLauncherPathFile))
            {
                return null;
            }

            try
            {
                string valueInFile = File.ReadAllText(nfhcLauncherPathFile).Trim();
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        // Delete the path so that the launcher should be used to launch Nitrox
                        File.Delete(nfhcLauncherPathFile);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Unable to delete the launcherpath.txt file. Nfhc will launch again without launcher. Error:{Environment.NewLine}{ex}");
                    }
                });
                return Directory.Exists(valueInFile) ? valueInFile : null;
            }
            catch
            {
                // ignored
            }
            return null;
        });

        public static void Execute()
        {
            Environment.SetEnvironmentVariable("NFHC_LAUNCHER_PATH", nfhcLauncherDir.Value);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += CurrentDomainOnAssemblyResolve;

            BootstrapNfhc();
        }

        private static void BootstrapNfhc()
        {
            Assembly core = Assembly.Load(new AssemblyName("NfhcModel"));
            Type mainType = core.GetType("NfhcModel.Main");
            mainType.InvokeMember("Execute", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, null);
        }

        private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string dllFileName = args.Name.Split(',')[0];
            if (!dllFileName.EndsWith(".dll"))
            {
                dllFileName += ".dll";
            }

            // Load DLLs where Nitrox launcher is first, if not found, use Subnautica's DLLs.
            string dllPath = Path.Combine(nfhcLauncherDir.Value, dllFileName);
            if (!File.Exists(dllPath))
            {
                dllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), dllFileName);
            }

            if (!File.Exists(dllPath))
            {
                Console.WriteLine($"Nfhc dll missing: {dllPath}");
            }
            return Assembly.LoadFile(dllPath);
        }
    }
}
