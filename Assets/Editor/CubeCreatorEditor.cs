using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CubeCreator))]
public class CubeCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        CubeCreator cubeCreator = (CubeCreator)target;
        DrawDefaultInspector();

        //EditorGUILayout.HelpBox("This is a help box", MessageType.Info);
        if (GUILayout.Button("Generate Cubes"))
        {
            cubeCreator.GenerateCubes();
        }
        if (GUILayout.Button("Destroy Cubes"))
        {
            cubeCreator.DestroyCubes();
        }
    }
}
