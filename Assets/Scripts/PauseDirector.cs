using UnityEngine;
using UnityEngine.Playables;

public class PauseDirector : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;

    public void Pause()
    {
        //Debug.LogWarning("Pause function was called");

        if (director == null)
        {
            Debug.LogError("PauseDirector: director is NULL");
            return;
        }
/*
        Debug.LogWarning("Director object: " + director.gameObject.name);
        Debug.LogWarning("Director state before pause: " + director.state);

        director.Pause();
*/
        //Debug.LogWarning("Director state after pause: " + director.state);
    }

    public void Resume()
    {
     //   Debug.LogWarning("Resume function was called");

        if (director == null)
        {
            Debug.LogError("PauseDirector: director is NULL");
            return;
        }
/*
        Debug.LogWarning("Director reference exists");
        Debug.LogWarning("Director GameObject: " + director.gameObject.name);
        Debug.LogWarning("Director enabled: " + director.enabled);
        Debug.LogWarning("Director GameObject activeInHierarchy: " + director.gameObject.activeInHierarchy);
        Debug.LogWarning("Director state before resume: " + director.state);
*/
        PlayableAsset asset = director.playableAsset;

        if (asset == null)
        {
            Debug.LogError("PauseDirector: director.playableAsset is NULL");
        }
        else
        {
            Debug.LogWarning("Assigned TL on resume: " + asset.name);
        }

        director.Resume();

        //Debug.LogWarning("Director state after resume: " + director.state);
    }
}