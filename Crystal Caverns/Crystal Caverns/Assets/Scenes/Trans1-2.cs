using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadOnEnter : MonoBehaviour
{
    public string sceneToLoad = "Level 2";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) // Enter
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
