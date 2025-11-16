using System.Collections.Generic;
using UnityEngine.Events;

namespace OrbBoosts;

public static class UpgradeStore {
	internal static Dictionary<string, Dictionary<string, int>> PlayerStats = new() {
		{ "playerUpgradeStrength", new Dictionary<string, int>() },
		{ "playerUpgradeHealth", new Dictionary<string, int>() },
		{ "playerUpgradeSpeed", new Dictionary<string, int>() },
		{ "playerUpgradeStamina", new Dictionary<string, int>() },
		{ "playerUpgradeExtraJump", new Dictionary<string, int>() },
		{ "playerUpgradeRange", new Dictionary<string, int>() },
		{ "playerUpgradeCrouchRest", new Dictionary<string, int>() },
		{ "playerUpgradeTumbleWings", new Dictionary<string, int>() },
		{ "playerUpgradeLaunch", new Dictionary<string, int>() },
		{ "playerUpgradeMapPlayerCount", new Dictionary<string, int>() }
	};

	internal static readonly Dictionary<string, (float, float, float)> BoostDictionary = new() {
		{ "playerUpgradeStrength",       (0.02f, 0.08f, 0.20f) }, // 2	 8	 20
		{ "playerUpgradeHealth",         (0.02f, 0.08f, 0.20f) }, // 2	 8	 20
		{ "playerUpgradeSpeed",          (0.08f, 0.09f, 0.10f) }, // 8	 9	 10
		{ "playerUpgradeStamina",        (0.08f, 0.10f, 0.10f) }, // 8	 10  10
		{ "playerUpgradeExtraJump",      (0.10f, 0.15f, 0.10f) }, // 10	 15  10
		{ "playerUpgradeRange",          (0.10f, 0.15f, 0.10f) }, // 10	 15  10
		{ "playerUpgradeCrouchRest",     (0.10f, 0.10f, 0.08f) }, // 10	 10  8
		{ "playerUpgradeTumbleWings",    (0.10f, 0.09f, 0.08f) }, // 10	 9	 8
		{ "playerUpgradeLaunch",         (0.20f, 0.08f, 0.02f) }, // 20	 8	 2
		{ "playerUpgradeMapPlayerCount", (0.20f, 0.08f, 0.02f) }  // 20	 8	 2
			
	};
	internal static readonly List<(string, (float, float, float))> Boosts = [
		("playerUpgradeStrength",       BoostDictionary["playerUpgradeStrength"]),
		("playerUpgradeHealth",         BoostDictionary["playerUpgradeHealth"]),
		("playerUpgradeSpeed",          BoostDictionary["playerUpgradeSpeed"]),
		("playerUpgradeStamina",        BoostDictionary["playerUpgradeStamina"]),
		("playerUpgradeExtraJump",      BoostDictionary["playerUpgradeExtraJump"]),
		("playerUpgradeRange",          BoostDictionary["playerUpgradeRange"]),
		("playerUpgradeCrouchRest",     BoostDictionary["playerUpgradeCrouchRest"]),
		("playerUpgradeTumbleWings",    BoostDictionary["playerUpgradeTumbleWings"]),
		("playerUpgradeLaunch",         BoostDictionary["playerUpgradeLaunch"]),
		("playerUpgradeMapPlayerCount", BoostDictionary["playerUpgradeMapPlayerCount"])
	];
	//playerUpgradeThrow : UpgradePlayerThrowStrength

	private static readonly UnityEvent<string> UpgradePlayerHealthEvent = new(); 
	private static readonly UnityEvent<string> UpgradePlayerEnergyEvent = new(); 
	private static readonly UnityEvent<string> UpgradePlayerExtraJumpEvent = new(); 
	private static readonly UnityEvent<string> UpgradeMapPlayerCountEvent = new(); 
	private static readonly UnityEvent<string> UpgradePlayerTumbleLaunchEvent = new(); 
	private static readonly UnityEvent<string> UpgradePlayerTumbleWingsEvent = new(); 
	private static readonly UnityEvent<string> UpgradePlayerSprintSpeedEvent = new(); 
	private static readonly UnityEvent<string> UpgradePlayerCrouchRestEvent = new(); 
	private static readonly UnityEvent<string> UpgradePlayerGrabStrengthEvent = new(); 
	private static readonly UnityEvent<string> UpgradePlayerGrabRangeEvent = new(); 
	
	
	private static readonly UnityEvent<string> ResetUpgradePlayerHealthEvent = new(); 
	private static readonly UnityEvent<string> ResetUpgradePlayerEnergyEvent = new(); 
	private static readonly UnityEvent<string> ResetUpgradePlayerExtraJumpEvent = new(); 
	private static readonly UnityEvent<string> ResetUpgradeMapPlayerCountEvent = new(); 
	private static readonly UnityEvent<string> ResetUpgradePlayerTumbleLaunchEvent = new(); 
	private static readonly UnityEvent<string> ResetUpgradePlayerTumbleWingsEvent = new(); 
	private static readonly UnityEvent<string> ResetUpgradePlayerSprintSpeedEvent = new(); 
	private static readonly UnityEvent<string> ResetUpgradePlayerCrouchRestEvent = new(); 
	private static readonly UnityEvent<string> ResetUpgradePlayerGrabStrengthEvent = new(); 
	private static readonly UnityEvent<string> ResetUpgradePlayerGrabRangeEvent = new(); 
	

	static UpgradeStore() {
		UpgradePlayerHealthEvent.AddListener(UpgradePlayerHealth);
		UpgradePlayerEnergyEvent.AddListener(UpgradePlayerEnergy);
		UpgradePlayerExtraJumpEvent.AddListener(UpgradePlayerExtraJump);
		UpgradeMapPlayerCountEvent.AddListener(UpgradePlayerMapPlayerCount);
		UpgradePlayerTumbleLaunchEvent.AddListener(UpgradePlayerTumbleLaunch);
		UpgradePlayerTumbleWingsEvent.AddListener(UpgradePlayerTumbleWings);
		UpgradePlayerSprintSpeedEvent.AddListener(UpgradePlayerSprintSpeed);
		UpgradePlayerCrouchRestEvent.AddListener(UpgradePlayerCrouchRest);
		UpgradePlayerGrabStrengthEvent.AddListener(UpgradePlayerGrabStrength);
		UpgradePlayerGrabRangeEvent.AddListener(UpgradePlayerGrabRange);
		
		ResetUpgradePlayerHealthEvent.AddListener(ResetUpgradePlayerHealth);
		ResetUpgradePlayerEnergyEvent.AddListener(ResetUpgradePlayerEnergy);
		ResetUpgradePlayerExtraJumpEvent.AddListener(ResetUpgradePlayerExtraJump);
		ResetUpgradeMapPlayerCountEvent.AddListener(ResetUpgradeMapPlayerCount);
		ResetUpgradePlayerTumbleLaunchEvent.AddListener(ResetUpgradePlayerTumbleLaunch);
		ResetUpgradePlayerTumbleWingsEvent.AddListener(ResetUpgradePlayerTumbleWings);
		ResetUpgradePlayerSprintSpeedEvent.AddListener(ResetUpgradePlayerSprintSpeed);
		ResetUpgradePlayerCrouchRestEvent.AddListener(ResetUpgradePlayerCrouchRest);
		ResetUpgradePlayerGrabStrengthEvent.AddListener(ResetUpgradePlayerGrabStrength);
		ResetUpgradePlayerGrabRangeEvent.AddListener(ResetUpgradePlayerGrabRange);
	}
	
	
	internal static readonly Dictionary<string, UnityEvent<string>> BoostActions = new() {
		{ "playerUpgradeHealth", UpgradePlayerHealthEvent },
		{ "playerUpgradeStamina", UpgradePlayerEnergyEvent },
		{ "playerUpgradeExtraJump", UpgradePlayerExtraJumpEvent },
		{ "playerUpgradeMapPlayerCount", UpgradeMapPlayerCountEvent },
		{ "playerUpgradeLaunch", UpgradePlayerTumbleLaunchEvent },
		{ "playerUpgradeTumbleWings", UpgradePlayerTumbleWingsEvent },
		{ "playerUpgradeSpeed", UpgradePlayerSprintSpeedEvent },
		{ "playerUpgradeCrouchRest", UpgradePlayerCrouchRestEvent },
		{ "playerUpgradeStrength", UpgradePlayerGrabStrengthEvent },
		{ "playerUpgradeRange", UpgradePlayerGrabRangeEvent }
	};
	
	internal static readonly Dictionary<string, UnityEvent<string>> ResetActions = new() {
		{ "playerUpgradeHealth", ResetUpgradePlayerHealthEvent },
		{ "playerUpgradeStamina", ResetUpgradePlayerEnergyEvent },
		{ "playerUpgradeExtraJump", ResetUpgradePlayerExtraJumpEvent },
		{ "playerUpgradeMapPlayerCount", ResetUpgradeMapPlayerCountEvent },
		{ "playerUpgradeLaunch", ResetUpgradePlayerTumbleLaunchEvent },
		{ "playerUpgradeTumbleWings", ResetUpgradePlayerTumbleWingsEvent },
		{ "playerUpgradeSpeed", ResetUpgradePlayerSprintSpeedEvent },
		{ "playerUpgradeCrouchRest", ResetUpgradePlayerCrouchRestEvent },
		{ "playerUpgradeStrength", ResetUpgradePlayerGrabStrengthEvent },
		{ "playerUpgradeRange", ResetUpgradePlayerGrabRangeEvent }
	};
	
	private static void UpgradePlayerHealth(string steamID) {
		if (!PlayerStats["playerUpgradeHealth"].ContainsKey(steamID)) {
			PlayerStats["playerUpgradeHealth"].Add(steamID, StatsManager.instance.playerUpgradeHealth[steamID]);
		}
		OrbBoosts.Logger.LogInfo("Health");
		PunManager.instance.UpgradePlayerHealth(steamID);
	}
	private static void UpgradePlayerEnergy(string steamID) {
		if (!PlayerStats["playerUpgradeStamina"].ContainsKey(steamID)) {
			PlayerStats["playerUpgradeStamina"].Add(steamID, StatsManager.instance.playerUpgradeStamina[steamID]);
		}
		OrbBoosts.Logger.LogInfo("Energy");
		PunManager.instance.UpgradePlayerEnergy(steamID);
	}
	private static void UpgradePlayerExtraJump(string steamID) {
		if (!PlayerStats["playerUpgradeExtraJump"].ContainsKey(steamID)) {
			PlayerStats["playerUpgradeExtraJump"].Add(steamID, StatsManager.instance.playerUpgradeExtraJump[steamID]);
		}
		OrbBoosts.Logger.LogInfo("ExtraJump");
		PunManager.instance.UpgradePlayerExtraJump(steamID);
	}
	private static void UpgradePlayerMapPlayerCount(string steamID) {
		if (!PlayerStats["playerUpgradeMapPlayerCount"].ContainsKey(steamID)) {
			PlayerStats["playerUpgradeMapPlayerCount"].Add(steamID, StatsManager.instance.playerUpgradeMapPlayerCount[steamID]);
		}
		OrbBoosts.Logger.LogInfo("MapPlayerCount");
		PunManager.instance.UpgradeMapPlayerCount(steamID);
	}
	private static void UpgradePlayerTumbleLaunch(string steamID) {
		if (!PlayerStats["playerUpgradeLaunch"].ContainsKey(steamID)) {
			PlayerStats["playerUpgradeLaunch"].Add(steamID, StatsManager.instance.playerUpgradeLaunch[steamID]);
		}
		OrbBoosts.Logger.LogInfo("TumbleLaunch");
		PunManager.instance.UpgradePlayerTumbleLaunch(steamID);
	}
	private static void UpgradePlayerTumbleWings(string steamID) {
		if (!PlayerStats["playerUpgradeTumbleWings"].ContainsKey(steamID)) {
			PlayerStats["playerUpgradeTumbleWings"].Add(steamID, StatsManager.instance.playerUpgradeTumbleWings[steamID]);
		}
		OrbBoosts.Logger.LogInfo("TumbleWings");
		PunManager.instance.UpgradePlayerTumbleWings(steamID);
	}
	private static void UpgradePlayerSprintSpeed(string steamID) {
		if (!PlayerStats["playerUpgradeSpeed"].ContainsKey(steamID)) {
			PlayerStats["playerUpgradeSpeed"].Add(steamID, StatsManager.instance.playerUpgradeSpeed[steamID]);
		}
		OrbBoosts.Logger.LogInfo("SprintSpeed");
		PunManager.instance.UpgradePlayerSprintSpeed(steamID);
	}
	private static void UpgradePlayerCrouchRest(string steamID) {
		if (!PlayerStats["playerUpgradeCrouchRest"].ContainsKey(steamID)) {
			PlayerStats["playerUpgradeCrouchRest"].Add(steamID, StatsManager.instance.playerUpgradeCrouchRest[steamID]);
		}
		OrbBoosts.Logger.LogInfo("CrouchRest");
		PunManager.instance.UpgradePlayerCrouchRest(steamID);
	}
	private static void UpgradePlayerGrabStrength(string steamID) {
		if (!PlayerStats["playerUpgradeStrength"].ContainsKey(steamID)) {
			PlayerStats["playerUpgradeStrength"].Add(steamID, StatsManager.instance.playerUpgradeStrength[steamID]);
		}
		OrbBoosts.Logger.LogInfo("GrabStrength");
		PunManager.instance.UpgradePlayerGrabStrength(steamID);
	}
	private static void UpgradePlayerGrabRange(string steamID) {
		if (!PlayerStats["playerUpgradeRange"].ContainsKey(steamID)) {
			PlayerStats["playerUpgradeRange"].Add(steamID, StatsManager.instance.playerUpgradeRange[steamID]);
		}
		OrbBoosts.Logger.LogInfo("GrabRange");
		PunManager.instance.UpgradePlayerGrabRange(steamID);
	}
	
	
	
	private static void ResetUpgradePlayerHealth(string steamID) {
		//OrbBoosts.Logger.LogInfo($"playerUpgradeHealth: {StatsManager.instance.playerUpgradeHealth[steamID]} : {PlayerStats["playerUpgradeHealth"][steamID]}");
		StatsManager.instance.playerUpgradeHealth[steamID] = PlayerStats["playerUpgradeHealth"][steamID];
	}
	private static void ResetUpgradePlayerEnergy(string steamID) {
		//OrbBoosts.Logger.LogInfo($"playerUpgradeStamina: {StatsManager.instance.playerUpgradeStamina[steamID]} : {PlayerStats["playerUpgradeStamina"][steamID]}");
		StatsManager.instance.playerUpgradeStamina[steamID] = PlayerStats["playerUpgradeStamina"][steamID];
	}
	private static void ResetUpgradePlayerExtraJump(string steamID) {
		//OrbBoosts.Logger.LogInfo($"playerUpgradeExtraJump: {StatsManager.instance.playerUpgradeExtraJump[steamID]} : {PlayerStats["playerUpgradeExtraJump"][steamID]}");
		StatsManager.instance.playerUpgradeExtraJump[steamID] = PlayerStats["playerUpgradeExtraJump"][steamID];
	}
	private static void ResetUpgradeMapPlayerCount(string steamID) {
		//OrbBoosts.Logger.LogInfo($"playerUpgradeMapPlayerCount: {StatsManager.instance.playerUpgradeMapPlayerCount[steamID]} : {PlayerStats["playerUpgradeMapPlayerCount"][steamID]}");
		StatsManager.instance.playerUpgradeMapPlayerCount[steamID] = PlayerStats["playerUpgradeMapPlayerCount"][steamID];
	}
	private static void ResetUpgradePlayerTumbleLaunch(string steamID) {
		//OrbBoosts.Logger.LogInfo($"playerUpgradeLaunch: {StatsManager.instance.playerUpgradeLaunch[steamID]} : {PlayerStats["playerUpgradeLaunch"][steamID]}");
		StatsManager.instance.playerUpgradeLaunch[steamID] = PlayerStats["playerUpgradeLaunch"][steamID];
	}
	private static void ResetUpgradePlayerTumbleWings(string steamID) {
		//OrbBoosts.Logger.LogInfo($"playerUpgradeTumbleWings: {StatsManager.instance.playerUpgradeTumbleWings[steamID]} : {PlayerStats["playerUpgradeTumbleWings"][steamID]}");
		StatsManager.instance.playerUpgradeTumbleWings[steamID] = PlayerStats["playerUpgradeTumbleWings"][steamID];
	}
	private static void ResetUpgradePlayerSprintSpeed(string steamID) {
		//OrbBoosts.Logger.LogInfo($"playerUpgradeSpeed: {StatsManager.instance.playerUpgradeSpeed[steamID]} : {PlayerStats["playerUpgradeSpeed"][steamID]}");
		StatsManager.instance.playerUpgradeSpeed[steamID] = PlayerStats["playerUpgradeSpeed"][steamID];
	}
	private static void ResetUpgradePlayerCrouchRest(string steamID) {
		//OrbBoosts.Logger.LogInfo($"playerUpgradeCrouchRest: {StatsManager.instance.playerUpgradeCrouchRest[steamID]} : {PlayerStats["playerUpgradeCrouchRest"][steamID]}");
		StatsManager.instance.playerUpgradeCrouchRest[steamID] = PlayerStats["playerUpgradeCrouchRest"][steamID];
	}
	private static void ResetUpgradePlayerGrabStrength(string steamID) {
		//OrbBoosts.Logger.LogInfo($"playerUpgradeStrength: {StatsManager.instance.playerUpgradeStrength[steamID]} : {PlayerStats["playerUpgradeStrength"][steamID]}");
		StatsManager.instance.playerUpgradeStrength[steamID] = PlayerStats["playerUpgradeStrength"][steamID];
	}
	private static void ResetUpgradePlayerGrabRange(string steamID) {
		//OrbBoosts.Logger.LogInfo($"playerUpgradeRange: {StatsManager.instance.playerUpgradeRange[steamID]} : {PlayerStats["playerUpgradeRange"][steamID]}");
		StatsManager.instance.playerUpgradeRange[steamID] = PlayerStats["playerUpgradeRange"][steamID];
	}
	
	
	
}