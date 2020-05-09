using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[ExecuteInEditMode]
public class Billboard : MonoBehaviour
{

    [SerializeField]
    private bool isRotating3D = false;

    void Update()
    {
        if (isRotating3D)
        {
            transform.LookAt(Camera.current.transform);
        }
        else
        {
            //Debug.Log(Camera.allCamerasCount);
            //Debug.Log(Camera.current.name);
            if (Camera.current != null)
            {
                Vector3 targetPosition = new Vector3(Camera.current.transform.position.x, transform.position.y, Camera.current.transform.position.z);
                transform.LookAt(targetPosition);
            }
        }
    }
}
