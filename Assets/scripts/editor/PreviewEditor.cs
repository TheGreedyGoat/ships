using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Preview))]

public class PreviewEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Preview preview = (Preview)target;
        if (GUILayout.Button("Preview Mesh"))
        {
            preview.GetPreviewFromSerialized();
        }
        if (GUILayout.Button("Preview Mesh O n Target"))
        {
            preview.PreviewOnTarget();
        }


        if (GUILayout.Button("Bake SHip With Cannon Placholders"))
        {
            preview.ShipBody();
        }
    }
}
