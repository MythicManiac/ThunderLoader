namespace Mythic.ThunderLoader
{
	public class ModInfo
	{
		public ModManifestV2 Manifest { get; protected set; }
		public string Path { get; protected set; }

		public string FullVersionName { get { return Manifest.FullVersionName; } }

		public string FullName { get { return Manifest.FullName; } }

		public bool IsThunderloaderMod
		{
			get
			{
				foreach (var loader in Manifest.Loaders)
				{
					if (loader.ToLowerInvariant().StartsWith("thunderloader"))
						return true;
				}
				return false;
			}
		}

		public bool IsBepinexMod
		{
			get
			{
				foreach (var loader in Manifest.Loaders)
				{
					if (loader.ToLowerInvariant() == "thunderloader.bepinex")
						return true;
				}
				return false;
			}
		}

		public bool IsMonoMod
		{
			get
			{
				foreach (var loader in Manifest.Loaders)
				{
					if (loader.ToLowerInvariant() == "thunderloader.monomod")
						return true;
				}
				return false;
			}
		}

		public ModInfo(ModManifestV2 manifest, string path)
		{
			Manifest = manifest;
			Path = path;
		}
	}
}
