using System.Collections.Generic;
using UnityEngine;

public class PSDestroy : MonoBehaviour
{
    private ParticleSystem vfx;
    private List<ParticleSystem> vfxList;
    private bool hasPlayed = false;

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
            if (ps == null)
                continue;

            if (ps.IsAlive(true))
            {
                //Debug.LogWarning($"{ps.name} is still alive", ps.gameObject);
                return; // at least one system is still playing/alive
            }
            
            hasPlayed = true;
        }
        
    if (hasPlayed) 
        Destroy(gameObject);
    }
}
