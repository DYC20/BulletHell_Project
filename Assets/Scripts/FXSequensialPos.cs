using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FXSequensialPos : MonoBehaviour
{
    [SerializeField] private List<Transform> sLightLocations; 
    [SerializeField] private List<VisualEffect> arrowLights;
    [SerializeField] private float flickerDuration;
    [SerializeField] private float zOffset;
    [SerializeField] private float timeOffset;
    
    private int currentLocationIndex = 0;
    private float timer = 0.0f;

    private Vector3 offset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        offset = new Vector3(0, 0, zOffset);
        timer = flickerDuration + timeOffset;
    }

    // Update is called once per frame
    void Update()
    {
        FollowS();
    }

    private void FollowS()
    {
        timer -= Time.deltaTime;
        transform.position = sLightLocations[currentLocationIndex].position + offset;
        transform.rotation = sLightLocations[currentLocationIndex].rotation;
        if (timer<=0f)
        {
            currentLocationIndex = (currentLocationIndex + 1) % sLightLocations.Count;
            timer = flickerDuration;
        }

        if (currentLocationIndex == 0)
        {
            
            for (int i = 0; i < arrowLights.Count; i++)
            {
                arrowLights[i].Play();
            }
        }
        Debug.Log("Count:"+sLightLocations.Count);
        Debug.Log("Index:" + currentLocationIndex);
    }
}
