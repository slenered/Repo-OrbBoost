using System.Linq;
using UnityEngine;

namespace OrbBoosts;

public class Unextractable : MonoBehaviour {
	private PhysGrabObjectImpactDetector _impactDetector = null!;
	private ValuableObject _valuableObject = null!;
	public bool becomesExtractable = true;
	public float unextractableTimer = 3f;
	
	private void Awake() {
		_impactDetector = GetComponent<PhysGrabObjectImpactDetector>();
		_valuableObject = GetComponent<ValuableObject>();
	}
	private void Update() {
		if (RoundDirector.instance.dollarHaulList.Contains(gameObject)) {
			RoundDirector.instance.dollarHaulList.Remove(gameObject);
		}

		foreach (var currentRoom in _valuableObject.roomVolumeCheck.CurrentRooms.Where(currentRoom => !currentRoom.Extraction)) {
			if (becomesExtractable) {
				unextractableTimer -= Time.deltaTime;
				if (unextractableTimer <= 0f) {
					// print("Extractable");
					_impactDetector.destroyDisable = false;
					Destroy(this);
				}
			}
		}
		
		if (becomesExtractable) {
			_impactDetector.destroyDisable = true;
		}
	}
}