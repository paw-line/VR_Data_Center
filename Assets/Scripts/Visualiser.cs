using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Visualiser : MonoBehaviour
{
    public abstract float Scan(out string visType, out string dataType, out string topic);

}
