using System;

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
        public string[] Loaders;
        public object ExtraData;

        public string FullVersionName { get { return $"{AuthorName}-{Name}-{Version}"; } }

        public string FullName { get { return $"{AuthorName}-{Name}"; } }
    }
}
