using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Water))]
public class WaterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        Water water = (Water)target;

        if(water.autoUpdate && EditorGUI.EndChangeCheck())
        {
            water.SetMaterial();
        }
        else if(!water.autoUpdate)
        {
            if(GUILayout.Button("Update Texture"))
            {
                water.SetMaterial();
            }
        }
    }
}
