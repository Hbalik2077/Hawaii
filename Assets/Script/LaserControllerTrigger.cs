using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserControllerTrigger : MonoBehaviour {
	public enum Type{Start,End

		}
	public Type type;
	bool isWorking = false;
	public LaserController LaserContrl;

    private void OnTriggerEnter(Collider other)
    {
		if (other.GetComponent<PlayerController>())
		{
			isWorking = true;
			if (type == Type.Start)
				LaserContrl.Working();
			else
				LaserContrl.Stop();
		}
	}
}
