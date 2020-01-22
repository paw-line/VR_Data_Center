using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataSource : MonoBehaviour
{
    [SerializeField]
    private float data = 0;
    [SerializeField]
    private float radius = 0;
    [SerializeField]
    private string type = null;

    public float GetData()
    {
        return data;
    }

    public float GetRadius()
    {
        return radius;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
