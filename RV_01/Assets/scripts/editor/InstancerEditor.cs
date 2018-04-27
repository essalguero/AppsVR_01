using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Instancer))]
public class InstancerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Instancer instance = (Instancer)target;
        if (GUILayout.Button("Instantiate"))
        {
            instance.Instantiate();
        }

        EditorUtility.SetDirty(target);
    }

}
