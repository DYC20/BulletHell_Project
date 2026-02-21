using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "FloatVariable", menuName = "Variables/Float Variable")]
public partial class FloatVariable : VariableResource<float>
{
    public float value
    {
        get => Value;
        set
        {
            Value = value;
        }
    }
    
}
[System.Serializable]
public class FloatReference : VeriableReference<float>
{
    
}
#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(FloatReference))]
public class FloatReferenceDrower: VeriableReferenceDrawer
{
    
}

#endif