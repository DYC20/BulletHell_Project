using UnityEngine;

public class ReplaceMaterial : MonoBehaviour
{
    //[SerializeField]private Material currentMaterial;
    [SerializeField]private Material assignMaterial;

    private SpriteRenderer spriteRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        //spriteRenderer.material = currentMaterial;
    }

    public void ChangeMaterial(Material newMaterial)
    {
        spriteRenderer.material = newMaterial;
    }
}
