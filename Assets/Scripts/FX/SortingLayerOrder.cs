using UnityEngine;

public class SortingLayerOrder : MonoBehaviour
{
    [SerializeField] private ParticleSystem bgParticleSystem;

    public void RaiseLayerOrderByOne()
    {
        ParticleSystemRenderer psRenderer = gameObject.GetComponent<ParticleSystemRenderer>();
        ParticleSystemRenderer bgRenderer = bgParticleSystem.GetComponent<ParticleSystemRenderer>();
        
        psRenderer.sortingOrder = bgRenderer.sortingOrder + 1;
    }
    
}
