using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManagerUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).name = "" + (i + 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).name = "" + (i + 1);
            }
        }
    }
}
