
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using LethalConfig;
using LethalConfig.ConfigItems;
using LethalLib.Modules;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace KatanaMod
{
  [BepInPlugin("APM.KatanaMod", "KatanaItemMod", "0.1.0")]
  public class Plugin : BaseUnityPlugin
  {
    private const string GUID = "APM.KatanaMod";
    private const string NAME = "KatanaItemMod";
    private const string VERSION = "0.1.0";
    private const string GUID2 = "ainavt.lc.lethalconfig";
    public static Plugin instance;
    private static AssetBundle bundle;
    private static ConfigEntry<bool> config_KatanaModRarity;
    private static string[] DisableMoonsList = new string[1]
    {
      "None"
    };

    public static bool IsAvailable => Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig");

    private void Awake()
    {
      Plugin.instance = this;
      this.Logger.LogInfo((object) ("LethalConfig Available: " + Plugin.IsAvailable.ToString()));
      Plugin.config_KatanaModRarity = this.Config.Bind<bool>("Spawn", "Enable Item Spawning", false, new ConfigDescription("Check this box to disable item spawning", (AcceptableValueBase) null, Array.Empty<object>()));
      string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "katanamod");
      Plugin.bundle = AssetBundle.LoadFromFile(assetDir);
      this.LoadScrap(assetDir, "Assets/LethalCompany/Mods/Items/APKatana/KatanaItem.asset", "Katana", 35);
      Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "APM.KatanaMod");
      this.Logger.LogInfo((object) "Loaded KatanaMod");
    }

    private void LoadScrap(string assetDir, string innerAssetPath, string name, int rarity)
    {
      ConfigEntry<int> entry = this.Config.Bind<int>(new ConfigDefinition("General", name + " Rarity"), rarity, new ConfigDescription("How often " + name + " will show up. Higher value means more common", (AcceptableValueBase) new AcceptableValueRange<int>(0, 100), Array.Empty<object>()));
      Item obj = Plugin.bundle.LoadAsset<Item>(innerAssetPath);
      NetworkPrefabs.RegisterNetworkPrefab(obj.spawnPrefab);
      Utilities.FixMixerGroups(obj.spawnPrefab);
      if (Plugin.config_KatanaModRarity.Value)
        Items.RegisterScrap(obj, entry.Value, (Levels.LevelTypes) 1, Plugin.DisableMoonsList);
      else
        Items.RegisterScrap(obj, entry.Value, Levels.LevelTypes.All);
      if (Plugin.IsAvailable)
        Plugin.AddIntSlider(entry, true);
      this.Logger.LogInfo((object) (name + " Loaded, Rarity: " + entry.Value.ToString()));
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void AddIntSlider(ConfigEntry<int> entry, bool restartRequired)
    {
      LethalConfigManager.AddConfigItem((BaseConfigItem) new IntSliderConfigItem(entry, restartRequired));
    }
  }
}
