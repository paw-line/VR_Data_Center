using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/*
[Serializable]
public struct Pairs
{
    public float threshold;           ///< Т
    public Color col;
}

[Serializable]
public struct TempSettings
{
    public string name;           ///< Т
    public List<Pairs> set;
    public Gradient grad;
}
//*/

public class ExampleScript : MonoBehaviour
{
    public Gradient gradient;
    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;

    public Pairs wololo;
    public List<Pairs> set;
    public TempSettings seton;
    public TempSettings[] setonist;


    void Start()
    {
        gradient = new Gradient();
        //List<Pairs> reset = set;
        colorKey = new GradientColorKey[set.Capacity];
        alphaKey = new GradientAlphaKey[set.Capacity];
        for (int i = 0; i < set.Capacity; i++)
        {
            Pairs t = set[i];
            t.threshold -= set[0].threshold;
            t.threshold /= set[set.Capacity-1].threshold-set[0].threshold;
            if (t.threshold < 0f)
                t.threshold = 0f;
            if (t.threshold > 1.0f)
                t.threshold = 1.0f;
            colorKey[i].color = t.col;
            colorKey[i].time = t.threshold;
            alphaKey[i].alpha = t.col.a;
            alphaKey[i].time = t.threshold;
        }

        gradient.SetKeys(colorKey, alphaKey);

        float data = 60f;
        float f = (data - set[0].threshold) / set[set.Capacity - 1].threshold - set[0].threshold;
        //Debug.Log(gradient.Evaluate(f));
    }

    //*/
}
