using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UniversalTranslator))]
public class UniversalTranslatorConfigEditor : Editor
{
    public TextAsset importAsset = null;
    public string exportFileName = "";

    public bool typeToRussian = false;
    public bool generalTypeToUnit = false;
    public bool typeToGeneraType = false;


    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        UniversalTranslator tgt = (UniversalTranslator)target;
        DrawDefaultInspector();

        exportFileName = tgt.exportFileName;
        importAsset = tgt.importAsset;


        //EditorGUILayout.HelpBox("This is a help box", MessageType.Info);
        if (GUILayout.Button("Export Dictonary to filename"))
        {
            if (exportFileName == "")
            {
                Debug.LogError("Enter a filename");
            }
            else
            {
                bool done = false;
                //Условие активности только одной из булей. Вроде.
                if (true)//(((typeToRussian ^ generalTypeToUnit) ^ typeToGeneraType) && !(typeToRussian && generalTypeToUnit && typeToGeneraType))
                {
                    if (tgt.typeToRussian && !done)
                    {
                        Debug.Log($"Exporting typeToRussian Dictionary to text file {exportFileName}");
                        tgt.WriteDicConfig(tgt.typeToRussianDictionary, exportFileName);
                        done = true;
                    }
                    if (tgt.generalTypeToUnit && !done)
                    {
                        Debug.Log($"Exporting generalTypeToUnit Dictionary to text file {exportFileName}");
                        tgt.WriteDicConfig(tgt.generalTypeToUnitDictionary, exportFileName);
                        done = true;
                    }
                    if (tgt.typeToGeneraType && !done)
                    {
                        Debug.Log($"Exporting typeToGeneraType Dictionary to text file {exportFileName}");
                        tgt.WriteDicConfig(tgt.typeToGeneraTypeDictionary, exportFileName);
                        done = true;
                    }
                    if (done)
                        Debug.Log("Export complete");
                    else
                        Debug.LogError("No dictonary was chosen");
                }
                else
                {
                    Debug.LogError("Choose only one dictionary to export");
                }
            }
            
        }
        if (GUILayout.Button("Import Dictonaries from TextAsset"))
        {
            if (importAsset == null)
            {
                Debug.LogError("Choose an asset!");
            }
            else
            {
                bool done = false;
                if (tgt.typeToRussian && !done)
                {
                    Debug.Log("Importing typeToRussian Dictionary from text asset");
                    tgt.typeToRussianDictionary = tgt.ReadDicConfig(importAsset);
                    done = true;
                }
                if (tgt.generalTypeToUnit && !done)
                {
                    Debug.Log("Importing generalTypeToUnit Dictionary from text asset");
                    tgt.generalTypeToUnitDictionary = tgt.ReadDicConfig(importAsset);
                    done = true;
                }
                if (tgt.typeToGeneraType && !done)
                {
                    Debug.Log("Importing typeToGeneraType Dictionary from text asset");
                    tgt.typeToGeneraTypeDictionary = tgt.ReadDicConfig(importAsset);
                    done = true;
                }
                if (done)
                    Debug.Log("Import complete");
                else
                    Debug.LogError("No dictonary was chosen");
                
            }
            
        }
    }
}
