using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 1f;

    public static event Action<string> OnSceneLoadStarted;
    public static event Action<string> OnSceneLoadCompleted;

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

    public void LoadSceneByName(string name)
    {
        LoadScene(name);
    }

    public void LoadAdditiveSceneByName(string name)
    {
        LoadScene(name, true);
    }

    public void LoadScene(string sceneName, bool additive = false)
    {
        StartCoroutine(LoadSceneRoutine(sceneName, additive));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, bool additive)
    {
        OnSceneLoadStarted?.Invoke(sceneName);

        yield return StartCoroutine(Fade(1));

        AsyncOperation loadOp;
        if (!additive)
        {
            loadOp = SceneManager.LoadSceneAsync(sceneName);
        }
        else
        {
            loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }

        while (!loadOp.isDone)
        {
            yield return null;
        }

        yield return StartCoroutine(Fade(0));

        OnSceneLoadCompleted?.Invoke(sceneName);
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvas == null) yield break;

        fadeCanvas.blocksRaycasts = true;

        float startAlpha = fadeCanvas.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            fadeCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        fadeCanvas.alpha = targetAlpha;
        fadeCanvas.blocksRaycasts = targetAlpha > 0;
    }
}
