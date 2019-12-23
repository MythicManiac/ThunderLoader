using BepInEx;
using System.Collections.Generic;
using ThunderLib;

namespace Mythic.ThunderLoader
{
    [BepInPlugin("Mythic.ThunderLoader", "ThunderLoader", "0.1.0")]
    public class ThunderLoaderPlugin : BaseUnityPlugin
    {
        public static readonly string LoaderReference = "Mythic-ThunderLoader-0.1.0";
        public void Awake()
        {
            Logger.LogInfo("---- Initialized ThunderLoader ----");
            LoadBepinexMods();
        }

        public void LoadBepinexMods()
        {
            var mods = ModDiscovery.GetModsForLoader(LoaderReference);

            var bepinexMods = new List<ModInfo>();
            foreach(var mod in mods)
            {
                var args = mod.GetLoaderArgs(LoaderReference, true, false);
                if(args != null && args.ContainsKey("UseBepinex") && (bool)args["UseBepinex"])
                {
                    bepinexMods.Add(mod);
                }
            }

            Logger.LogInfo("Loading BepInEx mods...");
            var bepinexLoader = new BepinexLoader(Logger, bepinexMods);
            bepinexLoader.LoadMods();
            Logger.LogInfo($"Loaded {bepinexLoader.LoadedMods.Count} BepInEx mods!");
        }
    }
}
