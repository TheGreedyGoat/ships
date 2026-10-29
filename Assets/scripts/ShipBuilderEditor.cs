using UnityEngine;
using System.Collections;
using UnityEditor;
using NUnit.Framework.Constraints;

[CustomEditor(typeof(ShipBuilder))]
public class ShipBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ShipBuilder myTarget = (ShipBuilder)target;
        if (GUILayout.Button("Build ship"))
        {
            myTarget.BuildShip();
        }
    }
}