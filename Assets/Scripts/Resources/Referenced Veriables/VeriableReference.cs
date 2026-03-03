
[System.Serializable]
public abstract class VeriableReference<T>
{
    public bool useConstant = true;
    public T constantValue;
    public VariableResource<T> variable;

    public T Value => useConstant ? constantValue : variable.GetV();
}
