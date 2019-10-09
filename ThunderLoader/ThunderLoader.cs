using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;

namespace Mythic.ThunderLoader
{
	[BepInPlugin("Mythic.ThunderLoader", "ThunderLoader", "1.0.0")]
	public class ThunderLoaderPlugin : BaseUnityPlugin
	{
		public void Awake()
		{
			var modLoadPath = Path.GetFullPath("Mods");
			Directory.CreateDirectory(modLoadPath);
			Logger.LogInfo("---- Initialized ThunderLoader ----");
			Logger.LogInfo($"Mod discovery path: {modLoadPath}");
			LoadBepinexMods(modLoadPath);
		}

		public List<ModInfo> DiscoverMods(string path)
		{
			var mods = new List<ModInfo>();

			foreach (string modDirectory in Directory.GetDirectories(path))
			{
				// No manifest, no mod
				var manifestPath = Path.Combine(modDirectory, "manifest.json");
				if (!File.Exists(manifestPath))
					continue;

				var manifestString = File.ReadAllText(manifestPath);
				var manifest = JsonUtility.FromJson<ModManifestV2>(manifestString);
				var modInfo = new ModInfo(manifest, modDirectory);

				// Not a thunderloader mod
				if (!modInfo.IsThunderloaderMod)
					continue;

				Logger.LogInfo($"    Discovered [{manifest.FullVersionName}]");
				mods.Add(modInfo);
			}

			return mods;
		}

		public void VerifyDependencies(IEnumerable<ModInfo> mods, IEnumerable<ModInfo> availableMods)
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
					Logger.LogError($"Multiple versions of the mod {mod.FullName} are available.");
					Logger.LogError("This might lead to issues and we recommend you only install one version of the same mod.");
				}
			}

			foreach (var mod in mods)
			{
				foreach (var dependency in mod.Manifest.Dependencies)
				{
					var versionless = dependency.Substring(0, dependency.LastIndexOf('-'));

					// Dependency found, All OK
					if (availableModsByName.ContainsKey(dependency))
						continue;

					// Dependency found, but it's a different version
					else if (!availableModsByName.ContainsKey(dependency) && availableModsByName.ContainsKey(versionless))
					{
						var available = availableModsByName[versionless];
						Logger.LogWarning($"{mod.FullName} depends on a different version of a dependency: {dependency}");
						Logger.LogWarning($"Available version: {available.FullVersionName}");
						continue;
					}

					// Dependency not found
					else
					{
						Logger.LogWarning($"{mod.FullName} depends on a missing dependency: {dependency}");
						continue;
					}
				}
			}
		}

		public void LoadBepinexMods(string path)
		{
			Logger.LogInfo("Discovering mods...");
			var mods = DiscoverMods(path);
			Logger.LogInfo($"{mods.Count} mods discovered!");

			Logger.LogInfo("Verifying dependencies...");
			VerifyDependencies(mods, mods);

			Logger.LogInfo("Loading BepInEx mods...");
			var bepinexLoader = new BepinexLoader(Logger, mods);
			bepinexLoader.LoadMods();
			Logger.LogInfo($"Loaded {bepinexLoader.LoadedMods.Count} BepInEx mods!");
		}
	}
}
