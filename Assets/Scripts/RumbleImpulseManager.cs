using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class RumbleImpulseManager : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource rumble;
    [SerializeField] private CinemachineImpulseSource buildup;
    [SerializeField] private CinemachineImpulseSource breakImpulse;

    [SerializeField] private float rumbleInterval = 0.15f;
    [SerializeField] private float rumbleDuration = 1f;

    public void PlaySequence()
    {
        StartCoroutine(ImpulseSequence());
    }

    private IEnumerator ImpulseSequence()
    {
        // 1. RUMBLE LOOP
        float timer = 0f;

        while (timer < rumbleDuration)
        {
            rumble.GenerateImpulse(Random.insideUnitSphere * 0.2f + Vector3.left);
            yield return new WaitForSeconds(rumbleInterval);
            timer += rumbleInterval;
        }

        // 2. BUILD-UP
        buildup.GenerateImpulse(Vector3.left);

        yield return new WaitForSeconds(0.3f);

        // 3. BREAK
        breakImpulse.GenerateImpulse(Vector3.down);
    }
    
}
