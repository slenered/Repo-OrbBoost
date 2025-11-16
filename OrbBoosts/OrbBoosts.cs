using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

namespace OrbBoosts;

[BepInPlugin("slenered.OrbBoosts", "OrbBoosts", BuildInfo.Version)]
[BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID)]

public class OrbBoosts : BaseUnityPlugin {
	internal static OrbBoosts Instance { get; private set; } = null!;
	public new static ManualLogSource Logger => Instance._logger;
	private ManualLogSource _logger => base.Logger;
	private Harmony? Harmony { get; set; }
	public readonly Dictionary<string, float> StealthTimer = new Dictionary<string, float>();
	internal bool microphoneEnabledPrevious = false;

	public static ConfigEntry<int> SingleplayerTempUpgradesAmount = null!;
	public static ConfigEntry<int> MultiplayerTempUpgradesAmount = null!; 
	
	private void Awake() {
		Instance = this;
		
		gameObject.transform.parent = null;
		gameObject.hideFlags = HideFlags.HideAndDontSave;

		SingleplayerTempUpgradesAmount = Config.Bind("Boosts", "Singleplayer Temporary Upgrades", 2, new ConfigDescription("How many upgrades should you get each time you get a Upgrade Boost in singleplayer"));
		MultiplayerTempUpgradesAmount = Config.Bind("Boosts", "Multiplayer Temporary Upgrades", 1, new ConfigDescription("How many upgrades should you get each time you get a Upgrade Boost in multiplayer"));
		
		Patch();

		Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
	}

	private void Patch() {
		//<Vision>d__34 MoveNext
		Harmony ??= new Harmony(Info.Metadata.GUID);
		
		var enemyVisionInnerType = AccessTools.Inner(typeof(EnemyVision), "<Vision>d__34");
		var enemyVisionClassInner = AccessTools.Method(enemyVisionInnerType, "MoveNext");
		Harmony.Patch(enemyVisionClassInner, transpiler: new HarmonyMethod(AccessTools.Method(typeof(Patches), nameof(Patches.EnemyVisionVision))));
		
		Harmony.PatchAll(typeof(Patches));
	}

	internal void Unpatch() {
		Harmony?.UnpatchSelf();
	}

	internal static bool CanCharge(ItemBattery item) {
		var unrechargeable = item.GetComponent<Unrechargeable>();
		return unrechargeable == null && item.batteryLifeInt < item.batteryBars;
	}
	internal static GameObject Instantiate(GameObject original, Vector3 position, Quaternion rotation, string name) {
		var ret = Instantiate(original, position, rotation);
		var boosts = ret.GetOrAddComponent<Boosts>();
		//print(name);
		boosts.ItemName = name;
		boosts.EnemyName = name;
		
		return ret;
	}

	internal static GameObject InstantiateRoomObject(string prefabName, Vector3 position, Quaternion rotation, byte group = 0, object[] data = null!, string name = "Orb") {
		var ret = PhotonNetwork.InstantiateRoomObject(prefabName, position, rotation, group, data);
		var boosts = ret.GetOrAddComponent<Boosts>();
		//print(name);
		boosts.ItemName = name;
		boosts.EnemyName = name;
		
		return ret;
	}

	internal static void DestroyPhysGrabObject(PhysGrabObject physGrabObject) {
		try {
			// print($"Burp: {physGrabObject.name}");
			if (physGrabObject.gameObject.TryGetComponent(typeof(Unextractable), out _)) return;

			if (physGrabObject.gameObject.TryGetComponent(typeof(Boosts), out var boostOrbComponent)) {
				var boostOrb = (Boosts)boostOrbComponent;
				boostOrb.Extracted();
			}
			physGrabObject.DestroyPhysGrabObject();
		}
		catch (Exception ex) {
			Debug.LogError("DestroyPhysGrabObject: " + ex);
		}

	}

	// internal static void VisionTrigger(EnemyVision instance, int playerID, PlayerAvatar player, bool culled, bool playerNear, bool sawPhysObj = false) {
	// 	if (Instance.StealthTimer.TryGetValue(player.steamID, out var value) && value > 0f && !instance.VisionTriggered[playerID] && !sawPhysObj) {
	// 		return;
	// 	}
	// 	instance.VisionTrigger(playerID, player, culled, playerNear);
	// }
	// internal static bool test() {
	// 	return true;
	// }
	
	internal static bool VisionTrigger(EnemyVision instance, int playerID, PlayerAvatar player, bool sawPhysObj, bool flag4, bool playerNear) {
		return sawPhysObj | flag4 | playerNear && !(Instance.StealthTimer.TryGetValue(player.steamID, out var value) && value > 0f && !instance.VisionTriggered[playerID] && !sawPhysObj);
	}

	internal static string ReportStats(string text, KeyValuePair<string, int> stat) {
		var key = "playerUpgrade" + stat.Key.Replace(" ", "");
		if (!UpgradeStore.PlayerStats.TryGetValue(key, out var statPair)) 
			return $"{text}<b>{stat.Value}\n</b>";
		var isTemp = statPair.TryGetValue(PlayerController.instance.playerSteamID, out var playerStat);
		if (!isTemp) 
			playerStat = stat.Value;
		
		var temporaryStat = stat.Value - playerStat > 0 ? $" <color=#f36a62>+ {stat.Value - playerStat}</color>" : "";
		return $"{text}<b>{playerStat}{temporaryStat}\n</b>";
	}

	internal static int VisionsToTrigger(int toTrigger, PlayerAvatar player) {
		var value = Instance.StealthTimer.GetValueOrDefault(player.steamID, 0);
		return value > 0f ? 30 : toTrigger;
	}

	private void Update() {
		foreach (var steamID in StealthTimer.Keys.ToList().Where(invisKey => StealthTimer[invisKey] > 0f)) {
			var playerAvatar = SemiFunc.PlayerAvatarGetFromSteamID(steamID);
			if (!playerAvatar) return;
			StealthTimer[steamID] -= Time.deltaTime;
			StealthTimer[steamID] = Mathf.Max(0f, StealthTimer[steamID]);
			
			var visible = StealthTimer[steamID] <= 0f;
			if (PlayerAvatar.instance != playerAvatar)
				playerAvatar.playerAvatarVisuals.meshParent.SetActive(visible);
			playerAvatar.playerAvatarVisuals.playerAvatarRightArm.grabberClawParent.gameObject.SetActive(visible);
			playerAvatar.flashlightController.gameObject.SetActive(visible);
			
		}
	}

	internal static class Patches {
		
		[HarmonyPatch(typeof(EnemyValuable), nameof(EnemyValuable.Start))]
		[HarmonyPostfix]
		internal static void EnemyValuableStart(EnemyValuable __instance) {
			var boosts = __instance.GetOrAddComponent<Boosts>();
			boosts.EnemyValuable =  __instance;
		}

		[HarmonyPatch(typeof(GameDirector), nameof(GameDirector.SetStart))]
		[HarmonyPostfix]
		internal static void GameDirectorSetStart(GameDirector __instance) {
			WeaponStore.Init();
			foreach (var stat in UpgradeStore.PlayerStats) {
				foreach (var playerStat in stat.Value) {
					if (UpgradeStore.PlayerStats.ContainsKey(stat.Key))
						UpgradeStore.ResetActions[stat.Key].Invoke(playerStat.Key);
				}
				stat.Value.Clear();
			}
			foreach (var player in __instance.PlayerList) {
				if (player.steamID == null) continue;
				Instance.StealthTimer[player.steamID] = 0.1f;
			}
		}

		[HarmonyPatch(typeof(ValuableObject), nameof(ValuableObject.AddToDollarHaulList))]
		[HarmonyPrefix]
		internal static bool AddToDollarHaulList(ValuableObject __instance) {
			if (__instance.gameObject.TryGetComponent(typeof(Unextractable), out _)) {
				//print("NO");
				return false;
			}
			return true;
		}

		[HarmonyPatch(typeof(ValuableObject), nameof(ValuableObject.AddToDollarHaulListRPC))]
		[HarmonyPrefix]
		internal static bool AddToDollarHaulListRPC(ValuableObject __instance) {
			if (__instance.gameObject.TryGetComponent(typeof(Unextractable), out _)) {
				//print("NO");
				return false;
			}
			return true;
		}
		

		[HarmonyPatch(typeof(ExtractionPoint), nameof(ExtractionPoint.DestroyTheFirstPhysObjectsInHaulList))]
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> DestroyTheFirstPhysObjectsInHaulListTranspiler(IEnumerable<CodeInstruction> instructions) {
			return DestroyObjectsTranspiler(instructions);
		}
		[HarmonyPatch(typeof(ExtractionPoint), nameof(ExtractionPoint.DestroyAllPhysObjectsInHaulList))]
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> DestroyAllPhysObjectsInHaulListTranspiler(IEnumerable<CodeInstruction> instructions) {
			return DestroyObjectsTranspiler(instructions);
		}
		private static IEnumerable<CodeInstruction> DestroyObjectsTranspiler(IEnumerable<CodeInstruction> instructions) {
			return new CodeMatcher(instructions)
				.MatchForward(false, new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(PhysGrabObject), nameof(PhysGrabObject.DestroyPhysGrabObject))))
				.Set(OpCodes.Call, AccessTools.Method(typeof(OrbBoosts), nameof(DestroyPhysGrabObject)))
				.InstructionEnumeration();
		}
		

		[HarmonyPatch(typeof(StatsUI), nameof(StatsUI.Fetch))]
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> StatsUIFetch(IEnumerable<CodeInstruction> instructions) {
			var patch = new CodeMatcher(instructions)
				.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "<b>"))
				.RemoveInstruction()
				.SetOpcodeAndAdvance(OpCodes.Ldloc_S)
				// .Advance(1)
				.RemoveInstructions(6)
				.Insert(
					new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OrbBoosts), nameof(ReportStats)))
				)
				.InstructionEnumeration();
			
			// foreach (var inst in patch) {
			// 	print(inst);
			// }
			return patch;
		}

		[HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.PlayerDeathRPC))]
		[HarmonyPostfix]
		internal static void PlayerAvatarPlayerDeathRPC(PlayerAvatar __instance) {
			Instance.StealthTimer[__instance.steamID] = 0.1f;
		}
		
		[HarmonyPatch(typeof(EnemyParent), nameof(EnemyParent.Despawn))]
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> EnemyParentDespawnPatch(IEnumerable<CodeInstruction> instructions) {
			var instantiateGeneric = typeof(UnityEngine.Object)
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.First(m => m.Name == "Instantiate"
				            && m.IsGenericMethodDefinition
				            && m.GetParameters().Length == 3);
			
			return new CodeMatcher(instructions)
				.MatchForward(false, new CodeMatch(OpCodes.Call, instantiateGeneric.MakeGenericMethod(typeof(GameObject))))
				.RemoveInstruction()
				.Insert(
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Ldfld,
						AccessTools.Field(typeof(EnemyParent), nameof(EnemyParent.enemyName))),
					new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OrbBoosts), nameof(Instantiate)))
				)
				.MatchForward(false, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(PhotonNetwork), nameof(PhotonNetwork.InstantiateRoomObject))))
				.RemoveInstruction()
				.Insert(
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Ldfld,
						AccessTools.Field(typeof(EnemyParent), nameof(EnemyParent.enemyName))),
					new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OrbBoosts), nameof(InstantiateRoomObject)))
				)
				.InstructionEnumeration();
		}

		[HarmonyPatch(typeof(ChargingStation), nameof(ChargingStation.ChargeAreaCheck))]
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> ChargingStationChargeAreaCheck(IEnumerable<CodeInstruction> instructions) {
			Label label = default;
			var a = 0;
			return new CodeMatcher(instructions)
				.MatchForward(false, new CodeMatch(i => {
					if (i.opcode == OpCodes.Bge && i.operand is Label lbl && a != 0) {
						label = lbl;
						return true; 
					}
					if (i.opcode == OpCodes.Bge) a += 1;
					return false; 
				}))
				.Advance(-3)
				.RemoveInstructions(4)
				.Insert(
					new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OrbBoosts), nameof(CanCharge))),
					new CodeInstruction(OpCodes.Brfalse, label)
				)
				.InstructionEnumeration();
		}

		
		
		[HarmonyPatch(typeof(PlayerVoiceChat), nameof(PlayerVoiceChat.Update))]
		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> PlayerVoiceChatUpdate(IEnumerable<CodeInstruction> instructions) {
		return new CodeMatcher(instructions)
				.MatchForward(false, 
					new CodeMatch(OpCodes.Ldc_I4_1),
					new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(PlayerVoiceChat), nameof(PlayerVoiceChat.microphoneEnabled)))
				)
				.RemoveInstruction()
				.Insert(
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(PlayerIsSilent)))
				)
				.InstructionEnumeration();
		}
	
		internal static IEnumerable<CodeInstruction> EnemyVisionVision(IEnumerable<CodeInstruction> instructions) {
			var codeInstructions = instructions.ToList();
				var code = new CodeMatcher(codeInstructions)
				.MatchForward(false,
					new CodeMatch(OpCodes.Ldloc_S), //4
					new CodeMatch(OpCodes.Ldloc_S), //5
					new CodeMatch(OpCodes.Ldelem_U1),
					new CodeMatch(OpCodes.Ldloc_S) //35
				)
				.SetAndAdvance(OpCodes.Ldloc_1, null)
				.InsertAndAdvance(
					new CodeInstruction(OpCodes.Ldloc_S, 21),
					new CodeInstruction(OpCodes.Ldloc_S, 19),
					new CodeInstruction(OpCodes.Ldloc_S, 4)
				)
				.Advance(3)
				.RemoveInstruction()
				.Advance(1)
				.RemoveInstruction()
				.Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OrbBoosts), nameof(VisionTrigger))))
				
				// .MatchForward(false, new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(EnemyVision), nameof(EnemyVision.VisionTrigger))))
				// .Repeat(matcher => matcher
				// 	.RemoveInstruction()
				// 	.Insert(
				// 		new CodeInstruction(OpCodes.Ldloc_S, 4),
				// 		new CodeInstruction(OpCodes.Ldloc_S, 5),
				// 		new CodeInstruction(OpCodes.Ldelem_U1),
				// 		new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OrbBoosts), nameof(VisionTrigger)))
				// 		)
				// )
				.Start()
				.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(EnemyVision), nameof(EnemyVision.VisionsToTrigger))))
				.Repeat(matcher => matcher
					.Advance(1)
					.Insert(
						new CodeInstruction(OpCodes.Ldloc_S, 19),
						new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OrbBoosts), nameof(VisionsToTrigger)))
					)
				)
				.Start()
				.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(EnemyVision), nameof(EnemyVision.VisionsToTriggerCrouch))))
				.Advance(1)
				.Insert(
					new CodeInstruction(OpCodes.Ldloc_S, 19),
					new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OrbBoosts), nameof(VisionsToTrigger)))
				)
				.Start()
				.MatchForward(false, new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(EnemyVision), nameof(EnemyVision.VisionsToTriggerCrawl))))
				.Advance(1)
				.Insert(
					new CodeInstruction(OpCodes.Ldloc_S, 19),
					new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(OrbBoosts), nameof(VisionsToTrigger)))
				)
				.InstructionEnumeration();
				return code;
		}

		private static bool IsSilent(string steamID) {
			// if (Instance.SilentTimer.TryGetValue(steamID, out var value)) {
			if (Instance.StealthTimer.TryGetValue(steamID, out var value)) {
				return value <= 0f;
			}
			return true;
		}
		internal static bool PlayerIsSilent(PlayerVoiceChat instance) {
			// print("PlayerIsSilent");
			// if (Instance.SilentTimer.TryGetValue(instance.playerAvatar.steamID, out var value)) {
			if (Instance.StealthTimer.TryGetValue(instance.playerAvatar.steamID, out var value)) {
				return value <= 0f;
			}
			return true;
		}
		
		[HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.Land))]
		[HarmonyPrefix]
		internal static bool PlayerAvatarLandPatch(PlayerAvatar __instance) {
			return IsSilent(__instance.steamID);
		}
		
		[HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.Jump))]
		[HarmonyPrefix]
		internal static bool PlayerAvatarJumpPatch(PlayerAvatar __instance) {
			return IsSilent(__instance.steamID);
		}

		[HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.Footstep))]
		[HarmonyPrefix]
		internal static bool PlayerAvatarFootstepPatch(PlayerAvatar __instance) {
			return IsSilent(__instance.steamID);
		}

		[HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.Slide))]
		[HarmonyPrefix]
		internal static bool PlayerAvatarSlidePatch(PlayerAvatar __instance) {
			return IsSilent(__instance.steamID);
		}

		[HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.StandToCrouch))]
		[HarmonyPrefix]
		internal static bool PlayerAvatarStandToCrouchPatch(PlayerAvatar __instance) {
			return IsSilent(__instance.steamID);
		}

		[HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.CrouchToStand))]
		[HarmonyPrefix]
		internal static bool PlayerAvatarCrouchToStandPatch(PlayerAvatar __instance) {
			return IsSilent(__instance.steamID);
		}

		[HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.CrouchToCrawl))]
		[HarmonyPrefix]
		internal static bool PlayerAvatarStandToCrawlPatch(PlayerAvatar __instance) {
			return IsSilent(__instance.steamID);
		}

		[HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.CrawlToCrouch))]
		[HarmonyPrefix]
		internal static bool PlayerAvatarCrawlToCrouchPatch(PlayerAvatar __instance) {
			return IsSilent(__instance.steamID);
		}

		[HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.TumbleStart))]
		[HarmonyPrefix]
		internal static bool PlayerAvatarTumbleStartPatch(PlayerAvatar __instance) {
			return IsSilent(__instance.steamID);
		}

		[HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.TumbleStop))]
		[HarmonyPrefix]
		internal static bool PlayerAvatarTumbleStopPatch(PlayerAvatar __instance) {
			return IsSilent(__instance.steamID);
		}

		[HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.TumbleBreakFree))]
		[HarmonyPrefix]
		internal static bool PlayerAvatarTumbleBreakFreePatch(PlayerAvatar __instance) {
			return IsSilent(__instance.steamID);
		}
		
		
		[HarmonyPatch(typeof(ItemUpgradePlayerGrabStrength), nameof(ItemUpgradePlayerGrabStrength.Upgrade))]
		[HarmonyPostfix]
		internal static void ItemUpgradePlayerGrabStrengthUpgrade(ItemUpgradePlayerGrabStrength __instance) {
			var steamID =
				SemiFunc.PlayerGetSteamID(
					SemiFunc.PlayerAvatarGetFromPhotonID(__instance.itemToggle.playerTogglePhotonID));
			if (UpgradeStore.PlayerStats["playerUpgradeStrength"].ContainsKey(steamID))
				UpgradeStore.PlayerStats["playerUpgradeStrength"][steamID] += 1;
		}

		[HarmonyPatch(typeof(ItemUpgradePlayerHealth), nameof(ItemUpgradePlayerHealth.Upgrade))]
		[HarmonyPostfix]
		internal static void ItemUpgradePlayerHealthUpgrade(ItemUpgradePlayerHealth __instance) {
			var steamID =
				SemiFunc.PlayerGetSteamID(
					SemiFunc.PlayerAvatarGetFromPhotonID(__instance.itemToggle.playerTogglePhotonID));
			if (UpgradeStore.PlayerStats["playerUpgradeHealth"].ContainsKey(steamID))
				UpgradeStore.PlayerStats["playerUpgradeHealth"][steamID] += 1;
		}

		[HarmonyPatch(typeof(ItemUpgradePlayerSprintSpeed), nameof(ItemUpgradePlayerSprintSpeed.Upgrade))]
		[HarmonyPostfix]
		internal static void ItemUpgradePlayerSprintSpeedUpgrade(ItemUpgradePlayerSprintSpeed __instance) {
			var steamID =
				SemiFunc.PlayerGetSteamID(
					SemiFunc.PlayerAvatarGetFromPhotonID(__instance.itemToggle.playerTogglePhotonID));
			if (UpgradeStore.PlayerStats["playerUpgradeSpeed"].ContainsKey(steamID))
				UpgradeStore.PlayerStats["playerUpgradeSpeed"][steamID] += 1;
		}

		[HarmonyPatch(typeof(ItemUpgradePlayerTumbleLaunch), nameof(ItemUpgradePlayerTumbleLaunch.Upgrade))]
		[HarmonyPostfix]
		internal static void ItemUpgradePlayerTumbleLaunchUpgrade(ItemUpgradePlayerTumbleLaunch __instance) {
			var steamID =
				SemiFunc.PlayerGetSteamID(
					SemiFunc.PlayerAvatarGetFromPhotonID(__instance.itemToggle.playerTogglePhotonID));
			if (UpgradeStore.PlayerStats["playerUpgradeLaunch"].ContainsKey(steamID))
				UpgradeStore.PlayerStats["playerUpgradeLaunch"][steamID] += 1;
		}

		[HarmonyPatch(typeof(ItemUpgradePlayerTumbleWings), nameof(ItemUpgradePlayerTumbleWings.Upgrade))]
		[HarmonyPostfix]
		internal static void ItemUpgradePlayerTumbleWingsUpgrade(ItemUpgradePlayerTumbleWings __instance) {
			var steamID =
				SemiFunc.PlayerGetSteamID(
					SemiFunc.PlayerAvatarGetFromPhotonID(__instance.itemToggle.playerTogglePhotonID));
			if (UpgradeStore.PlayerStats["playerUpgradeTumbleWings"].ContainsKey(steamID))
				UpgradeStore.PlayerStats["playerUpgradeTumbleWings"][steamID] += 1;
		}

		[HarmonyPatch(typeof(ItemUpgradeMapPlayerCount), nameof(ItemUpgradeMapPlayerCount.Upgrade))]
		[HarmonyPostfix]
		internal static void ItemUpgradeMapPlayerCountUpgrade(ItemUpgradeMapPlayerCount __instance) {
			var steamID =
				SemiFunc.PlayerGetSteamID(
					SemiFunc.PlayerAvatarGetFromPhotonID(__instance.itemToggle.playerTogglePhotonID));
			if (UpgradeStore.PlayerStats["playerUpgradeMapPlayerCount"].ContainsKey(steamID))
				UpgradeStore.PlayerStats["playerUpgradeMapPlayerCount"][steamID] += 1;
		}
		
		[HarmonyPatch(typeof(ItemUpgradePlayerCrouchRest), nameof(ItemUpgradePlayerCrouchRest.Upgrade))]
		[HarmonyPostfix]
		internal static void ItemUpgradePlayerCrouchRestUpgrade(ItemUpgradeMapPlayerCount __instance) {
            var steamID =
                SemiFunc.PlayerGetSteamID(
                 	SemiFunc.PlayerAvatarGetFromPhotonID(__instance.itemToggle.playerTogglePhotonID));
            if (UpgradeStore.PlayerStats["playerUpgradeCrouchRest"].ContainsKey(steamID))
                UpgradeStore.PlayerStats["playerUpgradeCrouchRest"][steamID] += 1;
        }

		[HarmonyPatch(typeof(ItemUpgradePlayerEnergy), nameof(ItemUpgradePlayerEnergy.Upgrade))]
		[HarmonyPostfix]
		internal static void ItemUpgradePlayerEnergyUpgrade(ItemUpgradePlayerEnergy __instance) {
			var steamID =
				SemiFunc.PlayerGetSteamID(
					SemiFunc.PlayerAvatarGetFromPhotonID(__instance.itemToggle.playerTogglePhotonID));
			if (UpgradeStore.PlayerStats["playerUpgradeStamina"].ContainsKey(steamID))
				UpgradeStore.PlayerStats["playerUpgradeStamina"][steamID] += 1;
		}

		[HarmonyPatch(typeof(ItemUpgradePlayerExtraJump), nameof(ItemUpgradePlayerExtraJump.Upgrade))]
		[HarmonyPostfix]
		internal static void ItemUpgradePlayerExtraJumpUpgrade(ItemUpgradePlayerExtraJump __instance) {
			var steamID =
				SemiFunc.PlayerGetSteamID(
					SemiFunc.PlayerAvatarGetFromPhotonID(__instance.itemToggle.playerTogglePhotonID));
			if (UpgradeStore.PlayerStats["playerUpgradeExtraJump"].ContainsKey(steamID))
				UpgradeStore.PlayerStats["playerUpgradeExtraJump"][steamID] += 1;
		}

		[HarmonyPatch(typeof(ItemUpgradePlayerGrabRange), nameof(ItemUpgradePlayerGrabRange.Upgrade))]
		[HarmonyPostfix]
		internal static void ItemUpgradePlayerGrabRangeUpgrade(ItemUpgradePlayerGrabRange __instance) {
			var steamID =
				SemiFunc.PlayerGetSteamID(
					SemiFunc.PlayerAvatarGetFromPhotonID(__instance.itemToggle.playerTogglePhotonID));
			if (UpgradeStore.PlayerStats["playerUpgradeRange"].ContainsKey(steamID))
				UpgradeStore.PlayerStats["playerUpgradeRange"][steamID] += 1;
		}
		
	}
}