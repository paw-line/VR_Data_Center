using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoorVisualisator : MonoBehaviour
{
    [SerializeField]
    private DataSource source;
    [SerializeField]
    private float openAngle = -120f;
    [SerializeField]
    private float closeAngle = 0f;
    [SerializeField]
    private float openTime = 1f;
    [SerializeField]
    private bool markOpenedDoors = false;

    private Material material;
    private float curAngle;
    
    public void Open()
    {
        StartCoroutine(SmoothOpen());
    }

    public void Close()
    {
        StartCoroutine(SmoothClose());
    }

    IEnumerator SmoothOpen()
    {
        float delta = 0;
        float curnAngle = transform.rotation.y;
        while (Mathf.Abs(curAngle - openAngle) >= 0.1)
        {
            delta = Time.deltaTime * (openAngle - curAngle) / openTime;
            curAngle += delta;
            Debug.Log(curAngle);
            Vector3 rot = new Vector3(0, curAngle, 0);
            transform.Rotate(rot);
            yield return null;
        }

    }

    IEnumerator SmoothClose()
    {
        float delta = 0;
        float initialAngle = transform.rotation.y;
        while (Mathf.Abs(curAngle - closeAngle) >= 0.1)
        {
            delta = Time.deltaTime * (closeAngle - initialAngle) / openTime;
            curAngle += delta;
            Vector3 rot = new Vector3(0, curAngle, 0);
            transform.Rotate(rot);
            yield return null;
        }

    }

    private void Update()
    {
        
    }
    
}
