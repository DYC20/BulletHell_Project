using UnityEngine;
using System;

public abstract partial class VariableResource<T> : ScriptableObject
{
    [SerializeField] private bool debug = false;
    [SerializeField] private bool alwaysEmit = false;
    public Action ValueChanged;
    [SerializeField] private T _value;
    //private string name = "";

    protected T Value
    {
        get => _value;
        set
        {
            if (!alwaysEmit && _value != null && _value.Equals(value))
                return;

            _value = value;

            //name = name == "" ? this.name : name;

            if (debug) Debug.Log($"Value in {name} is: " + _value);
            ValueChanged?.Invoke();
        }
    }
    public T GetV()
    {
        return _value;
    }
}
