using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ThunderLib
{
    public static class ModInstallMode
    {
        public static readonly string Extract = "Extract";
        public static readonly string Managed = "Managed";
        public static readonly string None = "None";
    }

    public static class ModDiscovery
    {
        public static string GetModLoadPath()
        {
            // TODO: Read from a configuration file
            var modLoadPath = Path.GetFullPath("Mods");
            Directory.CreateDirectory(modLoadPath);
            Log($"Using mod discovery path: {modLoadPath}", ConsoleColor.DarkGreen);
            return modLoadPath;
        }

        public static List<ModInfo> DiscoverUnmanagedMods()
        {
            // TODO: Implement
            return new List<ModInfo>();
        }

        public static List<ModInfo> DiscoverManagedMods()
        {
            var mods = new List<ModInfo>();

            foreach (string modDirectory in Directory.GetDirectories(GetModLoadPath()))
            {
                // No manifest, no mod
                var manifestPath = Path.Combine(modDirectory, "manifest.json");
                if (!File.Exists(manifestPath))
                    continue;

                try
                {
                    var manifestString = File.ReadAllText(manifestPath);
                    var manifest = JsonConvert.DeserializeObject<ModManifestV2>(manifestString);
                    var modInfo = new ModInfo(manifest, modDirectory);

                    if (manifest.InstallMode == ModInstallMode.Managed)
                        mods.Add(modInfo);
                    else
                        Log($"    Skipping unmanaged mod {modInfo.FullVersionName} found in managed mod directory", ConsoleColor.Yellow);
                }
                catch (Exception e)
                {
                    Log($"    Failed to parse mod manifest {manifestPath}", ConsoleColor.Red);
                    Log(e.ToString(), ConsoleColor.Red);
                }
            }

            return mods;
        }

        public static List<ModInfo> DiscoverAllMods()
        {
            var mods = DiscoverUnmanagedMods();
            mods = mods.Concat(DiscoverManagedMods()).ToList();
            SortModsByDependencyOrder(mods);
            return mods;
        }

        public static List<ModInfo> GetModsForLoader(string loader)
        {
            Log($"Discovering mods for loader {loader}...", ConsoleColor.DarkGreen);

            var allMods = DiscoverAllMods();

            var loaderMods = new List<ModInfo>();
            foreach (var mod in allMods)
            {
                if (mod.Manifest.InstallMode == ModInstallMode.Managed && mod.HasLoader(loader, true))
                    Log($"    Discovered [{mod.FullVersionName}]", ConsoleColor.DarkGreen);
                else
                    Log($"    Skipping [{mod.FullVersionName}]", ConsoleColor.DarkYellow);
                loaderMods.Add(mod);
            }

            Log("Verifying dependencies...", ConsoleColor.DarkGreen);
            CheckDependencies(allMods, allMods);
            Log("Done!", ConsoleColor.DarkGreen);

            return loaderMods;
        }

        public static List<ModInfo> SortModsByDependencyOrder(List<ModInfo> mods)
        {
            var modsByName = new Dictionary<string, ModInfo>();
            foreach (var mod in mods)
            {
                modsByName.Add(mod.FullVersionName, mod);
                if (!modsByName.ContainsKey(mod.FullName))
                {
                    modsByName.Add(mod.FullName, mod);
                }
            }

            var result = new List<ModInfo>();
            foreach(var mod in mods)
            {
                SortMods(result, modsByName, mod);
            }
            return result.Distinct().ToList();
        }

        private static void SortMods(List<ModInfo> result, Dictionary<string, ModInfo> modsByName, ModInfo currentMod)
        {
            foreach (var dependency in currentMod.GetDependencies())
            {
                var versionless = dependency.Substring(0, dependency.LastIndexOf('-'));
                if (modsByName.ContainsKey(dependency))
                {
                    SortMods(result, modsByName, modsByName[dependency]);
                }
                else if (modsByName.ContainsKey(versionless))
                {
                    SortMods(result, modsByName, modsByName[versionless]);
                }
            }
            result.Add(currentMod);
        }

        public static void Log(object message, ConsoleColor? color = null)
        {
            var lastColor = Console.ForegroundColor;
            if (color != null)
                Console.ForegroundColor = color.Value;
            Console.WriteLine(message);
            if (color != null)
                Console.ForegroundColor = lastColor;
        }

        public static void CheckDependencies(IEnumerable<ModInfo> mods, IEnumerable<ModInfo> availableMods)
        {
            var availableModsByName = new Dictionary<string, ModInfo>();
            foreach (var mod in availableMods)
            {

                availableModsByName.Add(mod.FullVersionName, mod);
                if (!availableModsByName.ContainsKey(mod.FullName))
                {
                    availableModsByName.Add(mod.FullName, mod);
                }
                else
                {
                    Log($"Multiple versions of the mod {mod.FullName} are available.", ConsoleColor.Yellow);
                    Log("This might lead to issues and we recommend you only install one version of the same mod.", ConsoleColor.Yellow);
                }
            }

            foreach (var mod in mods)
            {
                foreach (var dependency in mod.GetDependencies())
                {
                    var versionless = dependency.Substring(0, dependency.LastIndexOf('-'));

                    // Dependency found, All OK
                    if (availableModsByName.ContainsKey(dependency))
                        continue;

                    // Dependency found, but it's a different version
                    else if (!availableModsByName.ContainsKey(dependency) && availableModsByName.ContainsKey(versionless))
                    {
                        var available = availableModsByName[versionless];
                        Log($"{mod.FullName} depends on a different version of a dependency: {dependency}", ConsoleColor.Yellow);
                        Log($"Available version: {available.FullVersionName}", ConsoleColor.Yellow);
                        continue;
                    }

                    // Dependency not found
                    else
                    {
                        Log($"{mod.FullName} depends on a missing dependency: {dependency}", ConsoleColor.Red);
                        continue;
                    }
                }
            }
        }
    }
}
