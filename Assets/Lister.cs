using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class Lister : MonoBehaviour
{
    public PointList Distributor = new PointList();
}
 
[System.Serializable]
public class Point
{
    public List<GameObject> list;
}

[System.Serializable]
public class PointList
{
    public List<Point> list;
}
