using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using UnityEngine;

namespace Mythic.ThunderLoader
{

	[Serializable]
	public class ModManifestV2
	{
		public string ManifestVersion;
		public string AuthorName;
		public string Name;
		public string DisplayName;
		public string Version;
		public string WebsiteURL;
		public string Description;
		public string GameVersion;
		public string[] Dependencies;
		public string[] OptionalDependencies;
		public string[] Incompatibilities;
		public string NetworkMode;
		public string PackageType;
		public string InstallMode;
		public string Loader;
		public object ExtraData;

		public string FullVersionName { get { return $"{AuthorName}-{Name}-{Version}"; } }

		public string FullName { get { return $"{AuthorName}-{Name}"; } }
	}

	[BepInPlugin("Mythic.ThunderLoader", "ThunderLoader", "1.0.0")]
	public class ThunderLoader : BaseUnityPlugin
	{
		public string ModLoadPath { get; private set; }

		public void Awake()
		{
			ModLoadPath = Path.GetFullPath("Mods");
			Directory.CreateDirectory(ModLoadPath);
			Logger.LogInfo("---- Initialized ThunderLoader ----");
			Logger.LogInfo($"Mod discovery path: {ModLoadPath}");
			Logger.LogInfo("Loading mods...");
			LoadMods();
		}

		public void LoadMods()
		{
			var modInfos = DiscoverMods<BaseUnityPlugin>(ModLoadPath);
			Logger.LogInfo($"{modInfos.Count} mods discovered!");
			var modsByName = new Dictionary<string, ModManifestV2>();
			var loadedMods = new HashSet<string>();
			foreach (var modInfo in modInfos)
			{
				modsByName.Add(modInfo.Key.FullVersionName, modInfo.Key);
				modsByName.Add(modInfo.Key.FullName, modInfo.Key);
				// TODO: Add info about conflicting versions
			}

			foreach (var kvp in modInfos)
			{
				foreach (var dependency in kvp.Key.Dependencies)
				{
					var versionless = dependency.Substring(0, dependency.LastIndexOf('-'));

					// Skip if already loaded
					if (loadedMods.Contains(dependency))
						continue;

					// Warn about mismatching version
					if (loadedMods.Contains(versionless))
					{
						Logger.LogWarning($"{kvp.Key.FullName} depends on a different version of a dependency: {dependency}");
						// TODO: Know what version is loaded and report that
						Logger.LogWarning($"Loaded version: Unknown");
						continue;
					}

					// Load exact match
					if (modsByName.ContainsKey(dependency))
					{
						var manifest = modsByName[versionless];
						var types = modInfos[manifest];
						LoadMod(manifest, types);
						loadedMods.Add(dependency);
						loadedMods.Add(versionless);
					}
					// Load matching mod with different version
					else if (modsByName.ContainsKey(versionless))
					{
						var manifest = modsByName[versionless];
						var types = modInfos[manifest];
						Logger.LogWarning($"{kvp.Key.FullName} depends on a different version of a dependency: {dependency}");
						Logger.LogWarning($"Loading {manifest.FullVersionName} instead");
						LoadMod(manifest, types);
						loadedMods.Add(dependency);
						loadedMods.Add(versionless);
					}
					else
					{
						Logger.LogWarning($"{kvp.Key.FullName} depends on a missing dependency: {dependency}");
					}
				}

				if (!(loadedMods.Contains(kvp.Key.FullName) || loadedMods.Contains(kvp.Key.FullVersionName)))
					LoadMod(kvp.Key, kvp.Value);
			}
		}

		public void LoadMod(ModManifestV2 manifest, List<Type> types)
		{
			Logger.LogInfo($"Loading mod {manifest.FullName}...");
			foreach (var modType in types)
			{
				Chainloader.ManagerObject.AddComponent(modType);
				Logger.LogInfo($"    Loaded {modType.ToString()}");
			}
		}

		public Dictionary<ModManifestV2, List<Type>> DiscoverMods<T>(string directory)
		{
			var pluginType = typeof(T);
			var mods = new Dictionary<ModManifestV2, List<Type>>();

			foreach (string modDirectory in Directory.GetDirectories(directory))
			{
				// No manifest, no mod
				var manifestPath = Path.Combine(modDirectory, "manifest.json");
				if (!File.Exists(manifestPath))
					continue;

				// TODO: FILTER OUT NON-BEPINEX MODS

				var manifestString = File.ReadAllText(manifestPath);
				var manifest = JsonUtility.FromJson<ModManifestV2>(manifestString);
				Logger.LogInfo($"Discovered mod {manifest.FullVersionName}");

				var types = new List<Type>();

				foreach (var dllPath in Directory.GetFiles(modDirectory, "*.dll", SearchOption.AllDirectories))
				{
					try
					{
						AssemblyName assemblyName = AssemblyName.GetAssemblyName(dllPath);
						Assembly assembly = Assembly.Load(assemblyName);

						foreach (Type type in assembly.GetTypes())
						{
							if (!type.IsInterface && !type.IsAbstract && pluginType.IsAssignableFrom(type))
								types.Add(type);
						}
					}
					catch (BadImageFormatException) { }
					catch (ReflectionTypeLoadException ex)
					{
						Logger.LogError($"Could not load \"{Path.GetFileName(dllPath)}\"!");
						Logger.LogDebug(TypeLoadExceptionToString(ex));
					}
				}

				if (types.Count > 0)
				{
					mods.Add(manifest, types);
				}
			}

			return mods;
		}

		private static string TypeLoadExceptionToString(ReflectionTypeLoadException ex)
		{
			StringBuilder sb = new StringBuilder();
			foreach (Exception exSub in ex.LoaderExceptions)
			{
				sb.AppendLine(exSub.Message);
				if (exSub is FileNotFoundException exFileNotFound)
				{
					if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
					{
						sb.AppendLine("Fusion Log:");
						sb.AppendLine(exFileNotFound.FusionLog);
					}
				}
				else if (exSub is FileLoadException exLoad)
				{
					if (!string.IsNullOrEmpty(exLoad.FusionLog))
					{
						sb.AppendLine("Fusion Log:");
						sb.AppendLine(exLoad.FusionLog);
					}
				}

				sb.AppendLine();
			}

			return sb.ToString();
		}
	}
}
