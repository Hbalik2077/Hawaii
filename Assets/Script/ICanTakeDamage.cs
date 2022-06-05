using UnityEngine;
using System.Collections;

public interface ICanTakeDamage {

	void TakeDamage (int damage, Vector2 force, GameObject instigator, Vector3 hitPoint);
}
