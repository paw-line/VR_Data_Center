using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCreator : MonoBehaviour
{
    [SerializeField]
    private GameObject node = null;

    [SerializeField]
    private Vector3 zoneSize;
    [SerializeField]
    private Vector3 density;


    private GameObject[,,] nodes = null;
    int xn, yn, zn = 0;

    // Start is called before the first frame update
    void Start()
    {
        //GenerateCubes();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateCubes()
    {
        //Vector3 zoneSize = this.transform.localScale;

        xn = (int)Mathf.Floor(zoneSize.x / density.x)+1;
        yn = (int)Mathf.Floor(zoneSize.y / density.y)+1;
        zn = (int)Mathf.Floor(zoneSize.z / density.z)+1;
        nodes = new GameObject[xn, yn, zn];

        float offset = node.transform.localScale.x / 2;
        Vector3 localoffset = this.transform.position + new Vector3(this.transform.localScale.x / 2, this.transform.localScale.y / 2, this.transform.localScale.y / 2);
        Vector3 currentCoord = new Vector3(offset, offset, offset);

        for (int i = 0; i < xn; i++)
        {
            for (int j = 0; j < yn; j++)
            {
                for(int k = 0; k < zn; k++)
                {
                    nodes[i, j, k] = Instantiate(node);
                    nodes[i, j, k].transform.SetParent(this.transform);
                    nodes[i, j, k].name = "Node[" + i.ToString() + "," + j.ToString() + "," + k.ToString() + "]";

                    nodes[i, j, k].transform.localPosition =  new Vector3(offset + i * density.x, offset + j * density.y, offset + k * density.z);

                    //nodes[i, j, k].transform.position = currentCoord + localoffset;
                    //currentCoord += new Vector3(0, 0, density.z);
                }
                //currentCoord = new Vector3(offset + i * density.x, offset + j * density.y, offset);
            }
            //currentCoord = new Vector3(offset + i * density.x, offset, offset);
        }

    }

    public void DestroyCubes()
    {
        for (int i = 0; i < xn; i++)
        {
            for (int j = 0; j < yn; j++)
            {
                for (int k = 0; k < zn; k++)
                {
                    DestroyImmediate(nodes[i, j, k]);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position+zoneSize/2, zoneSize);
    }
}
