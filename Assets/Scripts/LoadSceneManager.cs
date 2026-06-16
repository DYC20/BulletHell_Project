using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    public static LoadSceneManager Instance { get; private set; }
    
    [SerializeField] private string sceneName;
    [SerializeField] private SceneTransition sceneTransitionGO;

    private bool isLoading;

    public void SetSceneName(string electedSceneName)
    {
        if (isLoading) return;
        sceneName = electedSceneName;
    }
    
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    //called with signal
    
    public void LoadScene()
    {
        if (isLoading) return;

        StartCoroutine(LoadSceneRoutine());
    }

    private IEnumerator LoadSceneRoutine()
    {
        isLoading = true;
        //sceneTransitionGO.RefreshCamera();
        yield return sceneTransitionGO.TransitionOutRoutine();
        
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        if (sceneName == null)
            Debug.LogWarning("Scene not found: " + sceneName);
        
        //sceneTransitionGO.RefreshCamera();
        
        while (!loadOperation.isDone)
        {
            yield return null;
        }
        
        yield return null;

        // 3. Fade back to visible AFTER loading
        
        yield return sceneTransitionGO.TransitionInRoutine();

        isLoading = false;
    }

    private IEnumerator QuitGameRoutine()
    {
        isLoading = true;
        yield return sceneTransitionGO.TransitionOutRoutine();
        Application.Quit();
    }

    public void ExitGame()
    {
        StartCoroutine(QuitGameRoutine());
    }

}
