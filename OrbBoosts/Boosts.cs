using System;
using System.Collections.Generic;
using Photon.Pun;
using REPOLib.Modules;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace OrbBoosts;

public class Boosts : MonoBehaviour {
	private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
	private static readonly int FresnelColorID = Shader.PropertyToID("_FresnelColor");
	private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
	private static readonly int SmoothnessID = Shader.PropertyToID("_Smoothness");
	private static readonly int FresnelPowerID = Shader.PropertyToID("_FresnelPower");
	private ItemToggle _itemToggle = null!;
	private ItemAttributes _itemAttributes = null!;
	private PhotonView _photonView = null!;
	private ValuableObject _valuableObject = null!;
	internal EnemyValuable EnemyValuable = null!;
	// private bool _gold;
	// private bool _used;
	private float _usedTimer;
	private Color _orbColor;
	private Color _orbEmissionColor;
	private Color _orbFresnelColor;
	private Color _goldFresnelColor = new (0.6792453f, 0.542977f, 0f);
	private readonly Color _goldColor = new (0.8880594f, 1f, 0f, 0.682353f);
	
	private readonly Color _weaponColor = new (0f, 0, 0f, 0.3137255f);
	private readonly Color _weaponEmissionColor = new (0f, 0, 0f);
	private readonly Color _weaponFresnelColor = new (0f, 0f, 0f, 1f);
	
	private readonly Color _stealthColor = new (1f, 0, 0.6594234f, 0.07058824f);
	private readonly Color _stealthFresnelColor = new (1f, 1f, 1f, 0.4627451f);
	
	private readonly Color _boostColor = new (0f, 0.9372549f, 1f, 0.3137255f);

	private int _size;
	private string _spawnWeapon = "";
	public BoostState state = BoostState.Unused;
	public enum BoostState {
		Unused,
		Used,
		Golden,
		Weapon,
		Stealth
	}
	
	internal string ItemName = "Orb";
	internal string EnemyName = "None";

	internal void Start() {
		
		_size = this.name.Replace("(Clone)", "") switch {
			"Enemy Valuable - Big" => 3,
			"Enemy Valuable - Medium" => 2,
			_ => 1
		};
		
		_itemToggle = this.AddComponent<ItemToggle>();
		_itemToggle.onToggle = new UnityEvent();
		_itemAttributes = this.AddComponent<ItemAttributes>();
		_photonView = GetComponent<PhotonView>();
		_orbColor = EnemyValuable.outerMaterial.GetColor(BaseColorID);
		_orbEmissionColor = EnemyValuable.outerMaterial.GetColor(EmissionColorID);
		_orbFresnelColor = EnemyValuable.fresnelColorDefault;
		_valuableObject = GetComponent<ValuableObject>();
		EnemyValuable.outerMaterial.SetColor(BaseColorID, _boostColor);
		_itemToggle.disabled = true;
		
		_itemAttributes.item = ScriptableObject.CreateInstance<Item>();
		// _itemAttributes.item.itemName = $"{ItemName}'s Soul";
		// _itemAttributes.item.itemAssetName = name;
		_itemAttributes.item.itemType = SemiFunc.itemType.orb;
		_itemAttributes.item.value = ScriptableObject.CreateInstance<Value>();
		_itemAttributes.item.value.valueMax = 1;
		_itemAttributes.item.value.valueMin = 0;
		if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
		SetName($"{ItemName}'s");

		// print("Boost Applied");
	}

	internal void Update() {
		if (EnemyValuable.indestructibleLerp < 1f)
			return;
		if (state != BoostState.Unused) {
			if (_usedTimer <= 0)
				enabled = false;
			Remove();
			_usedTimer -= Time.deltaTime;
			Color color;
			var colorFresnel = _orbFresnelColor;
			switch (state) {
				case BoostState.Golden:
					colorFresnel = Color.Lerp(_goldFresnelColor, _orbFresnelColor, _usedTimer);
					color = Color.Lerp(_goldColor, _boostColor, _usedTimer);
				break;
				case BoostState.Weapon:
					// print("weapon");
					colorFresnel = Color.Lerp(_weaponFresnelColor, _orbFresnelColor, _usedTimer);
                    color = Color.Lerp(_weaponColor, _boostColor, _usedTimer);
                    var colorEmission = Color.Lerp(_weaponEmissionColor, _orbEmissionColor, _usedTimer);
					EnemyValuable.outerMaterial.SetColor(EmissionColorID, colorEmission);
                    EnemyValuable.outerMaterial.SetFloat(SmoothnessID, Mathf.Lerp(0f, 0.8f, _usedTimer));
				break;
				case BoostState.Stealth:
					// print("stealth");
					colorFresnel = Color.Lerp(_stealthFresnelColor, _orbFresnelColor, _usedTimer);
                    color = Color.Lerp(_stealthColor, _boostColor, _usedTimer);
				break;
				default:
					color = Color.Lerp(_orbColor, _boostColor, EnemyValuable.indestructibleCurve.Evaluate(_usedTimer));
				break;
			}
            EnemyValuable.outerMaterial.SetColor(FresnelColorID, colorFresnel);
			EnemyValuable.outerMaterial.SetColor(BaseColorID, color);
			return;
		}
		_itemToggle.disabled = false;
		if (!SemiFunc.IsMasterClientOrSingleplayer()) return;
		if (state == BoostState.Unused && _itemToggle.toggleState) {
			TriggerRandomEffect();
		}
	}

	private void TriggerRandomEffect() {
		if (state != BoostState.Unused) return;
		// _used = true;
		state = BoostState.Used;
		var playerAvatar = SemiFunc.PlayerAvatarGetFromPhotonID(_itemToggle.playerTogglePhotonID);
		
		// print(name);
		
		var rng = Random.value;
		// rng = 0.05f;
		print($"RNG: {rng} : {playerAvatar.playerName}");
		switch (rng) {
			case > 0.50f: // 50% Heal
				// print("Healing");
				rng = Random.value;
				var healAmount = Mathf.CeilToInt(20 * rng * _size + 10 * _size);
				HealBoost(_itemToggle.playerTogglePhotonID, healAmount);
				break;
			case > 0.25f: // 25% Upgrade
				// print("Upgrading");
				RandomUpgrade(_itemToggle.playerTogglePhotonID, Random.value);
				break;
			case > 0.15f: // 10% Stealth
				StealthBoost();
				break;
			case > 0.05f: // 10% Weapon
				// print("Weapon");
				RandomWeapon(Random.value);
				break;
			case <= 0.05f: // 05% Gold
				GoldBoost();
				break;
		}

		if (SemiFunc.IsMultiplayer()) {
			_photonView.RPC("TriggerRandomEffectRPC", RpcTarget.All);
		} else {
			TriggerRandomEffectRPC();
		}
	}

	private void HealBoost(int playerID, int healAmount) {
		var playerAvatar = SemiFunc.PlayerAvatarGetFromPhotonID(playerID);
		playerAvatar.playerHealth.HealOther(healAmount, effect: false);
		
		if (!GameManager.Multiplayer() || playerAvatar.isLocal) {
			HealBoostRPC(playerID, healAmount);
			return;
		}
		_photonView.RPC("HealBoostRPC", RpcTarget.All, playerID, healAmount);
		// if (!playerAvatar.isLocal) return;
		// playerAvatar.playerHealth.HealOther(healAmount, effect: false);
		// if (healAmount >= 25f) 
		// 	CameraGlitch.Instance.PlayLongHeal();
		// else
		// 	CameraGlitch.Instance.PlayShortHeal();
	}
	
	[PunRPC]
	private void HealBoostRPC(int playerID, int healAmount) {
		var playerAvatar = SemiFunc.PlayerAvatarGetFromPhotonID(playerID);
		print($"Healing {healAmount} : {playerAvatar.playerName}");
		if (!playerAvatar.isLocal) return;
		if (healAmount >= 25f) 
			CameraGlitch.Instance.PlayLongHeal();
		else
			CameraGlitch.Instance.PlayShortHeal();
	}
	
	private void GoldBoost() {
		var rng = Mathf.Min(10 * Mathf.Pow(Mathf.Lerp(0.1f, 1f, Random.value) / 65, 0.55f), 1f);
		print($"GoldRNG: {rng}");
		var valueMultiplier = Mathf.Lerp(1.3f, 3f, rng);
		var newValue = _valuableObject.dollarValueOriginal * valueMultiplier;
		var howGolden = rng switch {
			<= 0.3f => "Slightly ",
			> 9f => "Extremely ",
			>= 0.7f => "Very ",
			_ => ""
		};

		SetName($"{EnemyName}'s {howGolden}Golden");
		
		if (SemiFunc.IsMultiplayer()) {
			_photonView.RPC("GoldBoostRPC", RpcTarget.All, rng, newValue);
		} else {
			GoldBoostRPC(rng, newValue);
		}
	}

	[PunRPC]
	private void GoldBoostRPC(float rng, float newValue) {
		// _goldColor = Color.Lerp(_orbColor, _goldColor, Mathf.Lerp(0.5f, 1f, rng));
		_goldFresnelColor = Color.Lerp(_orbFresnelColor, _goldFresnelColor, rng);
		_valuableObject.dollarValueCurrent = newValue;
		// _gold = true;
		state = BoostState.Golden;
	}

	private void SetValue(float value) {
		_valuableObject.dollarValueCurrent =  value;
	}
	
	private void StealthBoost() {
		SetState(BoostState.Stealth);
		SetName($"{EnemyName}'s Stealth");
		// EnemyValuable.indestructibleTimer += 1f;
		// EnemyValuable.impactDetector.destroyDisable = true;
		
		/*
		var droneObject = NetworkPrefabs.SpawnNetworkPrefab(NetworkPrefabs.GetNetworkPrefabRef("Items/Item Drone Temporary Stealth")!, transform.position, transform.rotation)!;
		var itemDroneTemporaryStealth = droneObject.GetComponent<ItemDroneTemporaryStealth>();
		itemDroneTemporaryStealth.StealthDuration = 40f * size;
		*/
	}
	
	private void RandomWeapon(float rng) {
		SetState(BoostState.Weapon);
		var weapons = WeaponStore.Weapons;
		weapons.Sort((tuple, valueTuple) => {
			var x = ParseWeight(tuple.Item2, _size);
			var y = ParseWeight(valueTuple.Item2, _size);
			return x.CompareTo(y);
		});
		
		var result = "";
		foreach (var weapon in weapons) {
			var weight = ParseWeight(weapon.Item2, _size);
			if (weight == 0f) continue;
			if (weight < 0f) break;
			if (weight >= rng) {
				result = weapon.Item1;
				break;
			}
			rng -= weight;
		}
		
		_spawnWeapon = result;
		SetName($"{EnemyName}'s Weapon");
		// print($"Planned Weapon: {_spawnWeapon}");
		SetValue(_valuableObject.dollarValueOriginal/2);
		// if (result != "") {
		// 	WeaponStore.SpawnItem(result, transform.position, transform.rotation, EnemyValuable.impactDetector.physGrabObject.playerGrabbing);
		// 	EnemyValuable.hasExplosion = false;
		// 	EnemyValuable.impactDetector.destroyDisable = false;
		// 	EnemyValuable.impactDetector.DestroyObject();
		// }
	}

	private void RandomUpgrade(int playerID, float rng) {
		var playerAvatar = SemiFunc.PlayerAvatarGetFromPhotonID(playerID);
		var upgrades = UpgradeStore.Boosts;
		upgrades.Sort((tuple, valueTuple) => {
			var x = ParseWeight(tuple.Item2, _size);
			var y = ParseWeight(valueTuple.Item2, _size);
			return x.CompareTo(y);
		});
		
		var steamID = SemiFunc.PlayerGetSteamID(playerAvatar);
		var addWeight = 0f;
		if (StatsManager.instance.playerUpgradeMapPlayerCount.ContainsKey(steamID) &&
			StatsManager.instance.playerUpgradeMapPlayerCount[steamID] > 0) {
			addWeight += ParseWeight(UpgradeStore.BoostDictionary["playerUpgradeMapPlayerCount"], _size) / (UpgradeStore.BoostDictionary.Count - 1);
		}
		
		var upgradeTimes = GameManager.Multiplayer() ? OrbBoosts.MultiplayerTempUpgradesAmount.Value : OrbBoosts.SingleplayerTempUpgradesAmount.Value;

		for (var i = 0; i < upgradeTimes; i++) {
			var result = "";
			foreach (var upgrade in upgrades) {
				var weight = ParseWeight(upgrade.Item2, _size) + addWeight;
				if (weight <= 0f) break;
				if (weight >= rng) {
					result = upgrade.Item1;
					break;
				}

				rng -= weight;
			}

			if (result != "") {
				// UpgradeStore.BoostActions[result].Invoke(steamID);

				if (!GameManager.Multiplayer()) {
					RandomUpgradeRPC(playerID, result);
					return;
				}

				_photonView.RPC("RandomUpgradeRPC", RpcTarget.All, playerID, result);
			}
		}
	}
	
	[PunRPC]
	private void RandomUpgradeRPC(int playerID, string upgradeName, PhotonMessageInfo _info = default(PhotonMessageInfo)) {
		var playerAvatar = SemiFunc.PlayerAvatarGetFromPhotonID(playerID);
		var steamID = SemiFunc.PlayerGetSteamID(playerAvatar);
		if (!SemiFunc.MasterOnlyRPC(_info)) return;
		UpgradeStore.BoostActions[upgradeName].Invoke(steamID);
		if (playerAvatar.isLocal) {
			// print($"PlayerAvatar: {playerAvatar.playerName}");
			StatsUI.instance.Fetch();
			StatsUI.instance.ShowStats();
			CameraGlitch.Instance.PlayUpgrade();
		} else 
			GameDirector.instance.CameraImpact.ShakeDistance(5f, 1f, 6f, base.transform.position, 0.2f);
		if (!GameManager.Multiplayer() || PhotonNetwork.IsMasterClient)
			playerAvatar.playerHealth.MaterialEffectOverride(PlayerHealth.Effect.Upgrade);
		
	}

	[PunRPC]
	private void TriggerRandomEffectRPC(PhotonMessageInfo _info = default(PhotonMessageInfo)) {
		Remove();
		_usedTimer = 2f;
	}
	private void Remove() {
		_itemToggle.disabled = true;
		// Destroy(_itemToggle);
		// Destroy(_itemAttributes);
		// _used =  true;
		if (state == BoostState.Unused) state = BoostState.Used;
	}
	
	private static float ParseWeight((float, float, float) weight, int size) {
		return size switch {
			1 => weight.Item1,
			2 => weight.Item2,
			3 => weight.Item3,
			_ => -1f
		};
	}

	private void SetName(string orbName) {
		if (!GameManager.Multiplayer()) {
			SetNameRPC(orbName);
			return;
		}
		_photonView.RPC("SetNameRPC", RpcTarget.All, orbName);
	}
	
	[PunRPC]
	private void SetNameRPC(string orbName) {
		this.ItemName = orbName;
		_itemAttributes.promptName = "";
		_itemAttributes.itemName = $"{ItemName} Soul";
		_itemAttributes.item.itemName = $"{ItemName} Soul";
		name = _itemAttributes.item.itemName;
		// _itemAttributes.item.itemAssetName = name;
	}
	
	
	public void SetState(BoostState newState) {
		if (SemiFunc.IsMultiplayer()) {
			_photonView.RPC("SetStateRPC", RpcTarget.All, newState);
		} else {
			SetStateRPC(newState);
		}
	}

	[PunRPC]
	public void SetStateRPC(BoostState newState) {
		state = newState;
		// print(state);
	}

	public void Extracted() {
		// print($"State: {state}");
		switch (state) {
			case BoostState.Golden:
				var bagObject = SemiFunc.IsMultiplayer()
					? PhotonNetwork.InstantiateRoomObject("Valuables/Surplus Valuable - Small", transform.position + transform.up, transform.rotation) :
					Instantiate(AssetManager.instance.surplusValuableSmall, transform.position + transform.up, transform.rotation); 
				// NetworkPrefabs.SpawnNetworkPrefab(NetworkPrefabs.GetNetworkPrefabRef("Valuables/Surplus Valuable - Small")!, transform.position + transform.up, transform.rotation)!;
				bagObject.GetComponent<ValuableObject>().dollarValueOverride = (int)_valuableObject.dollarValueCurrent/2;
				bagObject.AddComponent<Unextractable>();
				// bagObject.GetComponent<SurplusValuable>().indestructibleTimer = 30f;
			break;
			case BoostState.Weapon:
				WeaponStore.SpawnItem(_spawnWeapon, transform.position, transform.rotation, EnemyValuable.impactDetector.physGrabObject.playerGrabbing);
			break;
			case BoostState.Stealth:
				var droneObject = NetworkPrefabs.SpawnNetworkPrefab(NetworkPrefabs.GetNetworkPrefabRef("Items/Item Drone Temporary Stealth")!, transform.position + transform.up, transform.rotation)!;
				var itemDroneTemporaryStealth = droneObject.GetComponent<ItemDroneTemporaryStealth>();
				itemDroneTemporaryStealth.StealthDuration = 40f * _size;
			break;

			case BoostState.Unused:
			case BoostState.Used:
			default:
				return;
		}
	}
	
}