using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Vector3Variable", menuName = "Variables/Vector3 Variable")]
public partial class Vector3Variable : VariableResource<Vector3>
{
    public Vector3 value
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
public class Vector3Reference : VeriableReference<float>
{
    
}
#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(Vector3Reference))]
public class Vector3ReferenceDrower: VeriableReferenceDrawer
{
    
}

#endif
