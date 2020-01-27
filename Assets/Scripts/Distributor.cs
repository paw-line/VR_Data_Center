using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Distributor : MonoBehaviour
{
    public List<DataSource> sources;

    public static string rootTopic = "/dc";

    private void Awake()
    {
        DataSource[] temp;
        temp = FindObjectsOfType<DataSource>();
        sources = new List<DataSource>(temp);
    }
    //public List<Point> sources;
    //public List<DataSource> sources;

    //public Dictionary<string, DataSource> countries;   
}
/*
[System.Serializable]
public class Point
{
    public List<DataSource> sources;
}
*/
