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

    public void Set(float _data, string _type)
    {
        data = _data;
        type = _type;
    }

    public float GetData()
    {
        return data;
    }

    public float GetRadius()
    {
        return radius;
    }

    public string GetType()
    {
        return type;
    }

    private void Awake()
    {
        this.gameObject.name = Distributor.rootTopic + "/" + transform.parent.gameObject.name + "/" + this.name;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
