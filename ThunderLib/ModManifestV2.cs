using System;
using System.Collections.Generic;

namespace ThunderLib
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
        public Dictionary<string, Dictionary<string, object>> Loaders;
        public object ExtraData;

        public string VersionedFullName { get { return $"{AuthorName}-{Name}-{Version}"; } }

        public string FullName { get { return $"{AuthorName}-{Name}"; } }


        public Dictionary<string, object> GetLoaderArgs(string loader, bool greedy = false, bool warn = false)
        {
            if (Loaders.ContainsKey(loader))
                return Loaders[loader];

            var versionless = loader.Substring(0, loader.LastIndexOf('-'));

            foreach (var supportedLoader in Loaders.Keys)
            {
                var closeMatch = supportedLoader.Substring(0, supportedLoader.LastIndexOf('-'));
                if (closeMatch == versionless && greedy)
                {
                    if (warn)
                    {
                        ModDiscovery.Log($"{FullName} is being matched to a different version of the requested loader: {closeMatch}", ConsoleColor.Yellow);
                        ModDiscovery.Log($"Requested version: {supportedLoader}", ConsoleColor.Yellow);
                        ModDiscovery.Log($"Available version: {loader}", ConsoleColor.Yellow);
                    }
                    return Loaders[supportedLoader];
                }
            }
            return null;
        }

        public bool HasLoader(string loader, bool greedy = false)
        {
            return GetLoaderArgs(loader, greedy, true) != null;
        }
    }
}
