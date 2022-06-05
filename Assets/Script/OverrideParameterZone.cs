using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverrideParameterZone : MonoBehaviour {
	public bool canWalkOnThis = false;
	[Tooltip("Set layer for this zone, player can walk on it or not, leave it blank if don't want change the layer")]
	public string layerCanWalk = "Ground";

	[Header("------OVERRIDE PARAMETER------")]
	public PlayerState zone;
	public bool isOverridParameter = false;
	public PlayerParameter overrideParameter;
	[Header("-------FRICTION VALUE------")]
	[Header("* 0 -> 0.99: ICE EFFECT")]
	[Header("* 1.01-> 5: MUD EFFECT")]
	public bool isOverrideAcceleration = false;
	[Range(0,5)]
	public float overrideAcc = 1;


	[Header("------FORCE SPEED------")]
	public bool isUseAddForce=false;
	[Tooltip("if player stand -> translate player with this speed, if player move, add or subtract player's speed with this speed value")]
	public float forceMoveSpeed = 1;

	[Header("------WINDY ZONE------")]
	public float forceVertical = -0.2f;
	public float forceVeritcalWithParachute = 1.2f;
	void Start(){
		if (canWalkOnThis && layerCanWalk != "")
			gameObject.layer = LayerMask.NameToLayer (layerCanWalk);
	}
}
