using UnityEngine;
using UnityEngine.Playables;

public class PauseDirector : MonoBehaviour
{
[SerializeField] private PlayableDirector director;


public void Pause()
{
    director.Pause();
}

public void Resume()
{
    director.Resume();
}
}
