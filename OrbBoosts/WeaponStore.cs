using System.Collections.Generic;
using System.Linq;
using REPOLib.Modules;
using UnityEngine;

namespace OrbBoosts;

public static class WeaponStore {
	private static readonly Dictionary<string, (float, float, float)> WeaponDictionary = new() {
		{ "ItemGunTranq",          (2/9f , 0f, 0f) },
		{ "ItemGunStun",           (2/9f , 0f, 0f) },
		{ "ItemGunShockwave",      (2/9f , 0f, 0f) },
		{ "ItemMeleeFryingPan",    (1/9f , 0f, 0f) },
		{ "ItemMeleeBaseballBat",  (1/9f , 0f, 0f) },
		{ "ItemMeleeStunBaton",    (1/9f , 0f, 0f) },
		{ "ItemGunHandgun",        (0f, 1/2f, 0f) },
		{ "ItemMeleeSledgeHammer", (0f, 1/4f, 0f) },
		{ "ItemMeleeSword",        (0f, 1/4f, 0f) },
		{ "ItemGunLaser",          (0f, 0f, 1/3f) }, 
		{ "ItemGunShotgun",        (0f, 0f, 1/3f) },
		{ "ItemCartCannon",        (0f, 0f, 1/6f) },
		{ "ItemCartLaser",         (0f, 0f, 1/6f) }
			
	};
	
	internal static readonly List<(string, (float, float, float))> Weapons = [
		("ItemGunTranq",          WeaponDictionary["ItemGunTranq"]),
		("ItemGunStun",           WeaponDictionary["ItemGunStun"]),
		("ItemGunShockwave",      WeaponDictionary["ItemGunShockwave"]),
		("ItemMeleeFryingPan",    WeaponDictionary["ItemMeleeFryingPan"]),
		("ItemMeleeBaseballBat",  WeaponDictionary["ItemMeleeBaseballBat"]),
		("ItemMeleeStunBaton",    WeaponDictionary["ItemMeleeStunBaton"]),
		("ItemGunHandgun",        WeaponDictionary["ItemGunHandgun"]),
		("ItemMeleeSledgeHammer", WeaponDictionary["ItemMeleeSledgeHammer"]),
		("ItemMeleeSword",        WeaponDictionary["ItemMeleeSword"]),
		("ItemGunLaser",          WeaponDictionary["ItemGunLaser"]),
		("ItemGunShotgun",        WeaponDictionary["ItemGunShotgun"]),
		("ItemCartCannon",        WeaponDictionary["ItemCartCannon"]),
		("ItemCartLaser",         WeaponDictionary["ItemCartLaser"])
	];

	private static HashSet<PrefabRef> _items = null!;

	// internal static string StealthDrone = "";
	// internal static Item StealthDroneItem = null!;
	private static bool _droneItemInit;

	internal static void Init() {
		_items = StatsManager.instance.itemDictionary.Values.Select(x => x.prefab).OrderBy(x => x.PrefabName).ToHashSet();
		// foreach (var item in Valuables.AllValuables) {
		// 	OrbBoosts.Logger.LogInfo($"Name: {item.PrefabName} Path: {item.ResourcePath}");
		// }
		// _items = Resources.FindObjectsOfTypeAll<Item>().ToHashSet();
		if (_droneItemInit) return;
		_droneItemInit = true;
		
		// var itemDrone = _items.FirstOrDefault(x => x.itemAssetName == "Item Drone Temporary Stealth")!;
		// StealthDrone = "Items/Item Drone Temporary Stealth";
		// OrbBoosts.Logger.LogInfo(ResourcesHelper.GetItemPrefabPath(itemDrone));
	}

	// static WeaponStore() {
	// 	
	// }

	internal static void SpawnItem(string item, Vector3 position, Quaternion rotation, List<PhysGrabber>? playerGrabbing = null) {
		var itemObj = _items.FirstOrDefault(x => x.PrefabName.Replace(" ", "") == item);
		if (itemObj == null) {return;}
		var weapon = NetworkPrefabs.SpawnNetworkPrefab(itemObj!, position, rotation);
		weapon!.AddComponent<Unrechargeable>();
		var battery = weapon!.GetComponent<ItemBattery>();
		battery.isUnchargable = true;
		if (playerGrabbing == null) return;
		var grab = weapon!.GetComponent<PhysGrabObject>();
		grab.playerGrabbing =  playerGrabbing;
	}
	
}