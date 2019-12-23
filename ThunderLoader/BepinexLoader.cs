using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ThunderLib;

namespace Mythic.ThunderLoader
{
    public class BepinexLoader
    {
        public List<ModInfo> ModsToLoad { get; protected set; }

        public ManualLogSource Logger { get; protected set; }

        public HashSet<ModInfo> LoadedMods { get; protected set; }

        public BepinexLoader(ManualLogSource logger, List<ModInfo> modsToLoad)
        {
            Logger = logger;
            ModsToLoad = modsToLoad;
            LoadedMods = new HashSet<ModInfo>();
        }

        public void LoadMods()
        {
            foreach (var mod in ModsToLoad)
            {
                LoadMod(mod);
            }
        }

        public void LoadMod(ModInfo modInfo)
        {

            Logger.LogInfo($"Loading {modInfo.FullVersionName}...");
            foreach (var dllPath in Directory.GetFiles(modInfo.Path, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    AssemblyName assemblyName = AssemblyName.GetAssemblyName(dllPath);
                    Assembly assembly = Assembly.Load(assemblyName);

                    foreach (Type type in assembly.GetTypes())
                    {
                        if (!type.IsInterface && !type.IsAbstract && typeof(BaseUnityPlugin).IsAssignableFrom(type))
                        {
                            Chainloader.ManagerObject.AddComponent(type);
                            Logger.LogInfo($"    Loaded plugin {type.ToString()}");
                        }
                    }
                    LoadedMods.Add(modInfo);
                }
                catch (BadImageFormatException) { }
                catch (ReflectionTypeLoadException ex)
                {
                    Logger.LogError($"Could not load \"{Path.GetFileName(dllPath)}\"!");
                    Logger.LogDebug(Utils.TypeLoadExceptionToString(ex));
                }
            }
        }
    }
}
