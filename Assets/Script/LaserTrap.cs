using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTrap : MonoBehaviour {
	public Transform point1,point2;
	public float speedMoveIn = 1;
	public float speedMoveOut = 1;
	public float offsetPlayer = 5;
	public GameObject warningFX1;
	public GameObject warningFX2;

	float pos1,pos2;
	bool isMoveIn,isMoveOut;

	public LineRenderer lineRenderer;
	Transform target;
	BoxCollider boxCol;
	bool isWorking = false;

	void Start () {
		target = transform.parent.GetComponent<LaserController>().transform;
		pos1 =  10;
		pos2 = 10;
		boxCol = GetComponent<BoxCollider> ();
		boxCol.enabled = false;
		warningFX1.SetActive (false);
		warningFX2.SetActive (false);

		if (lineRenderer == null)
			lineRenderer = GetComponent<LineRenderer>();

		lineRenderer.enabled = false;
	}

	public void MoveIn(){
		pos1 = - 10;
		pos2 =  + 10;
        point1.localPosition = new Vector2(pos1, 0);
        point2.localPosition = new Vector2(pos2, 0);
        Invoke ("Work", 0.1f);
	}

	void Work(){
		isMoveIn = true;
		boxCol.enabled = false;
	}

	public void Warning(){
		warningFX1.SetActive (true);
		warningFX2.SetActive (true);
	}

	public void MoveOut(){
		isMoveOut = true;
		isMoveIn = false;
		StopLightning ();
	}

	public void DoLightning(float delay){
		warningFX1.SetActive (false);
		warningFX2.SetActive (false);
		
		lineRenderer.enabled = true;
		boxCol.enabled = true;
		isWorking = true;
	}

	public void StopLightning(){
		lineRenderer.enabled = false;
		boxCol.enabled = false;
		isWorking = false;
	}
	
	void FixedUpdate () {
		if (isWorking)
			lineRenderer.enabled = true;


		if (isMoveIn) {
			pos1 = Mathf.Lerp (pos1,  - offsetPlayer, speedMoveIn * Time.deltaTime);
			pos2 = Mathf.Lerp (pos2,  offsetPlayer, speedMoveIn * Time.deltaTime);
		} else if (isMoveOut) {
			pos1 = Mathf.Lerp (pos1,  - 2.5f * offsetPlayer, speedMoveOut * Time.deltaTime);
			pos2 = Mathf.Lerp (pos2, 2.5f * offsetPlayer, speedMoveOut * Time.deltaTime);
		} else {
			pos1 =  -2 * offsetPlayer;
			pos2 =  2 * offsetPlayer;
		}

		point1.localPosition = new Vector2 (pos1, 0);
		point2.localPosition = new Vector2 (pos2, 0);

		lineRenderer.SetPosition(0, point1.position);
		lineRenderer.SetPosition(1, point2.position);
	}
}
