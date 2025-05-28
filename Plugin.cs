using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using KindredCommands.Models;
using HarmonyLib;
using ProjectM;
using UnityEngine;
using VampireCommandFramework;

namespace KindredCommands;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.VampireCommandFramework")]
public class Plugin : BasePlugin
{
	internal static Harmony Harmony;
	internal static ManualLogSource PluginLog;
	public static ManualLogSource LogInstance { get; private set; }

	public override void Load()
	{
		if (Application.productName != "VRisingServer")
			return;

		PluginLog = Log;
		// Plugin startup logic
		Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
		LogInstance = Log;
		Database.InitConfig();
		// Harmony patching
		Harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
		Harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

		// Register all commands in the assembly with VCF
		CommandRegistry.RegisterAll();

		if (Core.GetWorld("Server") != null)
		{
			Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is reloaded!");
			Core.InitializeAfterLoaded();
		}
	}

	public override bool Unload()
	{
		CommandRegistry.UnregisterAssembly();
		Harmony?.UnpatchSelf();
		return true;
	}


	public void OnGameInitialized()
	{
		if (!HasLoaded())
		{
			Log.LogDebug("Attempt to initialize before everything has loaded.");
			return;
		}

		Core.InitializeAfterLoaded();
	}

	private static bool HasLoaded()
	{
		// Hack, check to make sure that entities loaded enough because this function
		// will be called when the plugin is first loaded, when this will return 0
		// but also during reload when there is data to initialize with.
		var collectionSystem = Core.Server.GetExistingSystemManaged<PrefabCollectionSystem>();
		return collectionSystem?.SpawnableNameToPrefabGuidDictionary.Count > 0;
	}
}
