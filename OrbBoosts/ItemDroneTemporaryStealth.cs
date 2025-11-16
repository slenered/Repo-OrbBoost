using System;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

namespace OrbBoosts;

public class ItemDroneTemporaryStealth : MonoBehaviour {
	private ItemToggle _itemToggle = null!;
	private ItemAttributes _itemAttributes = null!;
	//private PhotonView _photonView = null!;
	private ItemDrone _itemDrone = null!;
	private PhysGrabObject _myPhysGrabObject = null!;
	private ItemEquippable _itemEquippable = null!;
	private ItemBattery _itemBattery = null!;
	private Unrechargeable _unrechargeable = null!;
	private float _timeSince = 0f;
	
	internal float StealthDuration = 40f;
	
	internal void Start() {
		_itemToggle = GetComponent<ItemToggle>();
		_itemAttributes = GetComponent<ItemAttributes>();
		//_photonView = GetComponent<PhotonView>();
		// _itemAttributes.item = WeaponStore.StealthDroneItem;
		
		// GetComponentInChildren<SemiIconMaker>().gameObject.SetActive(true);
		
		_itemDrone = GetComponent<ItemDrone>();
		_myPhysGrabObject = GetComponent<PhysGrabObject>();
		_itemEquippable = GetComponent<ItemEquippable>();
		_itemBattery = GetComponent<ItemBattery>();
		_unrechargeable = GetComponent<Unrechargeable>();
		
		// _itemDrone.batteryDrainPreset = ScriptableObject.CreateInstance<BatteryDrainPresets>();
		_itemDrone.batteryDrainPreset.batteryDrainRate = _itemBattery.batteryLife / StealthDuration / 2; //6/StealthDuration;
		_itemBattery.batteryDrainRate = _itemDrone.batteryDrainPreset.batteryDrainRate;

	}

	internal void Update() {
		if (_itemEquippable.isEquipped) {
			return;
		}
		if (_itemDrone is { itemActivated: true, magnetActive: true } && (bool)_itemDrone.playerAvatarTarget) {
			var playerAvatar = _itemDrone.playerAvatarTarget;
			OrbBoosts.Instance.StealthTimer[playerAvatar.steamID] = 0.1f;
			// print($"{_itemBattery.batteryLife} : {_timeSince}");
			if (_timeSince >= StealthDuration)
				_unrechargeable.DestroyObject();
			_timeSince += Time.deltaTime;
		}
		if ((GameManager.instance.gameMode != 1 || PhotonNetwork.IsMasterClient) && _itemDrone.itemActivated) {
			_myPhysGrabObject.OverrideZeroGravity();
			_myPhysGrabObject.OverrideDrag(1f);
			_myPhysGrabObject.OverrideAngularDrag(10f);
		}
	}
}