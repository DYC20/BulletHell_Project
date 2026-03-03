
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "StringVariable", menuName = "Variables/String Variable")]
public partial class StringVariable : VariableResource<string>
{
    public string value
    {
        get
        {
            return Value;
        }
        set
        {
            Value = value;
        }
    }
}

[System.Serializable]
public class StringReference : VeriableReference<float>
{
    
}
#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(StringReference))]
public class StringReferenceDrower: VeriableReferenceDrawer
{
    
}

#endif
