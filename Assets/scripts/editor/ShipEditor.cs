using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Ship))]
public class ShipEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var ship = (Ship)target;
        if (ship.IsBaked)
        {
            if (GUILayout.Button("Assign As Player"))
            {
                ship.SetPlayer();
            }

            if (GUILayout.Button("Unbake"))
            {
                ship.Unbake();
            }
        }
        else if (GUILayout.Button("Bake"))
        {
            ship.Bake();
        }
    }
}
