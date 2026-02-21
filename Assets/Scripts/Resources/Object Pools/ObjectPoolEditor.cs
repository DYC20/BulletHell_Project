using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectPool))]
public class ObjectPoolEditor : Editor
{
    private void OnEnable()
    {
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/Scripts/Resources/Object Pools/ObjectPoolIcon.png"
        );

        if (icon != null)
        {
            EditorGUIUtility.SetIconForObject(target, icon);
        }
    }
}
