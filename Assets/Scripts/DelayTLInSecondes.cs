using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class DelayTLInSecondes : MonoBehaviour
{
    [SerializeField] PlayableDirector director;
    [SerializeField] float duration;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(DelayDirector(duration));
    }

    private IEnumerator DelayDirector(float seconds)
    {
        director.Pause();
        yield return new WaitForSeconds(seconds);
        director.Play();
    }
}
