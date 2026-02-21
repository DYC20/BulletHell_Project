using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "IntVariable", menuName = "Variables/Int Variable")]
public partial class IntVariable : VariableResource<int>
{
    public int value
    {
        get => Value;
        set
        {
            Value = value;
        }
    }
    public void Set(int vl) { value = vl; }
    public void Append(int vl) { value += vl; }
}

[System.Serializable]
public class IntReference : VeriableReference<float>
{
    
}
#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(IntReference))]
public class IntReferenceDrower: VeriableReferenceDrawer
{
    
}

#endif