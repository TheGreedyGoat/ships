using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Shipbaker))]
public class ShipBakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        // Shipbaker myTarget = (Shipbaker)target;
        // if (GUILayout.Button("Test"))
        // {
        //     myTarget.BakeMesh();
        // }

    }
}