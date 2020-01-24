// Create a foldable header menu that hides or shows the selected Transform position.
// If you have not selected a Transform, the Foldout item stays folded until
// you select a Transform.

using UnityEditor;
using UnityEngine;

public class FoldoutHeaderUsage : EditorWindow
{
    bool showPosition = true;
    string status = "Select a GameObject";

    [MenuItem("Examples/Foldout Header Usage")]
    static void CreateWindow()
    {
        GetWindow<FoldoutHeaderUsage>();
    }

    public void OnGUI()
    {
        // An absolute-positioned example: We make foldout header group and put it in a small rect on the screen.
        showPosition = EditorGUI.BeginFoldoutHeaderGroup(new Rect(10, 10, 200, 100), showPosition, status);

        if (showPosition)
            if (Selection.activeTransform)
            {
                Selection.activeTransform.position =
                    EditorGUI.Vector3Field(new Rect(10, 30, 200, 100), "Position", Selection.activeTransform.position);
                status = Selection.activeTransform.name;
            }

        if (!Selection.activeTransform)
        {
            status = "Select a GameObject";
            showPosition = false;
        }
        // End the Foldout Header that we began above.
        EditorGUI.EndFoldoutHeaderGroup();
    }
}