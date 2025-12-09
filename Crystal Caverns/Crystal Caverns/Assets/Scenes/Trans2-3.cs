using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadOnPortal : MonoBehaviour
{
    public string sceneToLoad = "Level 3";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
