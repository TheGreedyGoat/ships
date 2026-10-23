using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(Rigidbody))]
public class RigidbodyEditor : Editor
{
    void OnSceneGUI()
    {
        Rigidbody rb = target as Rigidbody;
        Handles.color = Color.red;
        Handles.DrawWireCube(rb.transform.TransformPoint(rb.centerOfMass), new Vector3(.3f, .3f, .3f));
    }
    public override void OnInspectorGUI()
    {
        GUI.skin = EditorGUIUtility.GetBuiltinSkin(UnityEditor.EditorSkin.Inspector);
        DrawDefaultInspector();
    }
}