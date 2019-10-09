namespace Mythic.ThunderLoader
{
	public class ModInfo
	{
		public ModManifestV2 Manifest { get; protected set; }
		public string Path { get; protected set; }

		public string FullVersionName { get { return Manifest.FullVersionName; } }

		public string FullName { get { return Manifest.FullName; } }

		public bool IsThunderloaderMod { get { return Manifest.Loader.ToLowerInvariant().StartsWith("thunderloader"); } }

		public bool IsBepinexMod { get { return Manifest.Loader.ToLowerInvariant() == "thunderloader.bepinex"; } }

		public bool IsMonoMod { get { return Manifest.Loader.ToLowerInvariant() == "thunderloader.monomod"; } }

		public ModInfo(ModManifestV2 manifest, string path)
		{
			Manifest = manifest;
			Path = path;
		}
	}
}
