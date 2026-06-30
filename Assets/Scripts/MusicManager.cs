using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private List<AudioSource> audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (AudioSource source in audioSource)
        {
            if (source == null) continue;
            source.loop = true;
            if (!source.isPlaying)
                source.Play();

        }
    }
    private void OnDisable()
    {
        foreach (AudioSource source in audioSource)
        {
            if (source == null)
                continue;

            source.Stop();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
