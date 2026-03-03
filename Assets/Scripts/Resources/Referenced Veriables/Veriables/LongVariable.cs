
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "LongVariable", menuName = "Variables/Long Variable")]
public partial class LongVariable : VariableResource<long>
{
    public long value
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

    public void Set(long vl) { value = vl; }
    public void Set(int vl) { value = vl; }
    public void Append(long vl) { value += vl; }
    public void Append(int vl) { value += vl; }
}

[System.Serializable]
public class LongReference : VeriableReference<float>
{
    
}
#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(LongReference))]
public class LongReferenceDrower: VeriableReferenceDrawer
{
    
}

#endif
