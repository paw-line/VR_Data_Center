using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]

public class Callout : MonoBehaviour
{

    [SerializeField]
    private Transform start = null;
    [SerializeField]
    private Transform finish = null;
    [SerializeField]
    private GameObject cylinder = null;

    void Update()
    {
        UpdateCylinderPosition(cylinder, start.position, finish.position);
    }

    

    private void UpdateCylinderPosition(GameObject cylinder, Vector3 beginPoint, Vector3 endPoint)
    {
        Vector3 offset = endPoint - beginPoint;
        Vector3 position = beginPoint + (offset / 2.0f);

        cylinder.transform.position = position;
        cylinder.transform.LookAt(beginPoint);
        Vector3 localScale = cylinder.transform.localScale;
        localScale.z = (endPoint - beginPoint).magnitude;
        cylinder.transform.localScale = localScale;
    }

}
