using System.Collections.Generic;
using System.Linq;

namespace ThunderLib
{
    public class ModInfo
    {
        public ModManifestV2 Manifest { get; protected set; }
        public string Path { get; protected set; }

        public string FullVersionName { get { return Manifest.VersionedFullName; } }

        public string FullName { get { return Manifest.FullName; } }


        public ModInfo(ModManifestV2 manifest, string path)
        {
            Manifest = manifest;
            Path = path;
        }

        public List<string> GetDependencies()
        {
            var result = new List<string>(Manifest.Dependencies);
            foreach(var kvp in Manifest.Loaders)
            {
                result.Add(kvp.Key);
            }
            return result;
        }

        public Dictionary<string, object> GetLoaderArgs(string loader, bool greedy = false, bool warn = false)
        {
            return Manifest.GetLoaderArgs(loader, greedy, warn);
        }

        public bool HasLoader(string loader, bool greedy = false)
        {
            return Manifest.HasLoader(loader, greedy);
        }
    }
}
