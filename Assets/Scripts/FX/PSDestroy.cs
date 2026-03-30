using System.Collections.Generic;
using UnityEngine;

public class PSDestroy : MonoBehaviour
{
    private ParticleSystem vfx;
    private List<ParticleSystem> vfxList;

    void Awake()
    {        
        ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>();

        vfxList = new List<ParticleSystem>(systems);
    }

    private void Start()
    {
        
    }
    
    void Update()
    {
      foreach (var ps in vfxList)
        {
            if (ps != null && ps.IsAlive(true))
                return; // at least one system still playing
        }

        Destroy(gameObject);
    }
}
