using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private List<ParticleSystem> pressFX;
    [SerializeField] private float duration;
    
    public void LoadSelectedScene()
    {
        //StartCoroutine(PlayFX);
        SceneManager.LoadScene(sceneName);
    }

    /*private IEnumerator PlayFX()
    {
        float time = duration;
        
        foreach (var effect in pressFX)
        {
            
        }
    }*/
}
