using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Mythic.ThunderLoader
{
    public class BepinexLoader
    {
        public HashSet<ModInfo> LoadedMods { get; protected set; }

        public List<ModInfo> ModsToLoad { get; protected set; }

        public Dictionary<string, ModInfo> ModsToLoadByName { get; protected set; }

        public ManualLogSource Logger { get; protected set; }

        public BepinexLoader(ManualLogSource logger, List<ModInfo> modsToLoad)
        {
            Logger = logger;
            LoadedMods = new HashSet<ModInfo>();

            ModsToLoad = modsToLoad;
            ModsToLoadByName = new Dictionary<string, ModInfo>();
            foreach (var mod in ModsToLoad)
            {
                ModsToLoadByName.Add(mod.FullVersionName, mod);
                if (!ModsToLoadByName.ContainsKey(mod.FullName))
                {
                    ModsToLoadByName.Add(mod.FullName, mod);
                }
            }
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
            if (LoadedMods.Contains(modInfo))
                return;

            if (!modInfo.IsBepinexMod)
                return;

            // Add to loaded mods already so we don't get infinite recursion
            LoadedMods.Add(modInfo);

            foreach (var dependency in modInfo.Manifest.Dependencies)
            {
                var versionless = dependency.Substring(0, dependency.LastIndexOf('-'));
                if (ModsToLoadByName.ContainsKey(dependency))
                {
                    LoadMod(ModsToLoadByName[dependency]);
                }
                else if (ModsToLoadByName.ContainsKey(versionless))
                {
                    LoadMod(ModsToLoadByName[versionless]);
                }
            }

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
