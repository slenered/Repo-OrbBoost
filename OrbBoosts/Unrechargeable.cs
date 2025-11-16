using System;
using Photon.Pun;
using UnityEngine;

namespace OrbBoosts;

public class Unrechargeable : MonoBehaviour {
	private PhysGrabObject _myPhysGrabObject = null!;
	private ItemBattery _itemBattery = null!;
	private readonly Color _batteryColor = new (0.3333333f, 0f, 0f);
	private float despawnTimer = 10f;

	private void Start() {
		_myPhysGrabObject = GetComponent<PhysGrabObject>();
		_itemBattery = _myPhysGrabObject.GetComponent<ItemBattery>();
		
		// _itemBattery.batteryVisualLogic.batteryBorderMain = 
	}

	private void Update() {
		if (_itemBattery.batteryColorMedium != _batteryColor) {
			var colorPreset = ScriptableObject.CreateInstance<ColorPresets>();
			colorPreset.colorDark = _itemBattery.itemAttributes.colorPreset.colorDark;
			colorPreset.colorLight = _itemBattery.itemAttributes.colorPreset.colorLight;
			colorPreset.colorMain = _batteryColor;
			_itemBattery.itemAttributes.colorPreset = colorPreset;
			_itemBattery.batteryColorMedium = _batteryColor;
		}

		if (_itemBattery.batteryLifeInt <= 0) {
			despawnTimer -= Time.deltaTime;
			if (despawnTimer <= 0)
				Destroy(gameObject);
		}
	}

	// public void PassComponents(PhysGrabObject physGrabObject, ItemBattery itemBattery) {
	// 	_myPhysGrabObject = physGrabObject;
	// 	_itemBattery = itemBattery;
	// 	// _itemEquippable = itemEquippable;
	// }

	public void DestroyObject(bool effects = true) {
		if (!SemiFunc.IsMasterClientOrSingleplayer() || _myPhysGrabObject.dead) return;
		_myPhysGrabObject.dead = true;
		if (!SemiFunc.IsMultiplayer()) {
			_myPhysGrabObject.impactDetector.DestroyObjectRPC(effects);
			return;
		}
		_myPhysGrabObject.impactDetector.photonView.RPC("DestroyObjectRPC", RpcTarget.All, effects);
	}
}