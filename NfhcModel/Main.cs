using Autofac;
using HarmonyLib;
using NfhcModel.Core;
using NfhcModel.DataStructures;
using NfhcModel.Logger;
using NfhcModel.Modules;
using NfhcModel.MonoBehaviours;
using NfhcModel.MonoBehaviours.Gui.MainMenu;
using NfhcModel.Patches;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NfhcModel
{
    public class Main
    {
        private static IContainer container;

        private static readonly Harmony harmony = new Harmony("com.nfhc.harmony");
        private static bool isApplied = false;

        public static void Execute()
        {
            if (!isApplied)
            {
                Log.Setup(true);
                Log.Info($"NFHC version {Assembly.GetExecutingAssembly().GetName().Version} built on {File.GetCreationTimeUtc(Assembly.GetExecutingAssembly().Location)}");

                Log.Info("Registering dependencies");
                container = CreatePatchingContainer();

                Log.Info("Container created..");

                try
                {
                    IAutoFacRegistrar[] registrars = { new ServerAutoFacRegistrar(), new ClientAutoFacRegistrar() };
                    NfhcServiceLocator.InitializeDependencyContainer(registrars);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Log.Error($"Failed to load one or more dependency types for Nitrox. Assembly: {ex.Types.FirstOrDefault()?.Assembly.FullName ?? "unknown"}");
                    foreach (Exception loaderEx in ex.LoaderExceptions)
                    {
                        Log.Error(loaderEx);
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while initializing and loading dependencies.");
                    throw;
                }

                InitPatches();
                ApplyNfhcBehaviours(); 
            }
        }

        private static void ApplyNfhcBehaviours()
        {
            Log.Info("Applying NFHC behaviours..");
            GameObject nfhcRoot = new GameObject();
            nfhcRoot.name = "Nfhc";
            nfhcRoot.AddComponent<NfhcBootstrapper>();
            Log.Info("Behaviours applied.");
        }

        private static void InitPatches()
        {
            Log.Debug($"Apply() in Nfhc Main!");
            Validate.NotNull(container, "No patches have been discovered yet! Run Execute() first.");
            if (isApplied)
            {
                return;
            }

            foreach (IDynamicPatch patch in container.Resolve<IDynamicPatch[]>())
            {
                Log.Debug($"Applying dynamic patch {patch.GetType().Name}");
                patch.Patch(harmony);
            }

            isApplied = true;
        }

        private static IContainer CreatePatchingContainer()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterModule(new NfhcPatchesModule());
            return builder.Build();
        }

    }
}
