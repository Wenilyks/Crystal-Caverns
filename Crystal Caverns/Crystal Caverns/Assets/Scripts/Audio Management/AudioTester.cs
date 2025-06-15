using UnityEngine;
using UnityEngine.InputSystem;

public class AudioTester : MonoBehaviour
{
    void Update()
    {
        // Test audio
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AudioManager.Instance.PlaySFX("Jump test");
        }

        // Test volume controls
        if (Input.GetKeyDown(KeyCode.M))
            AudioManager.Instance.SetMusicVolume(0.3f);

        if (Input.GetKeyDown(KeyCode.N))
            AudioManager.Instance.SetMusicVolume(1f);

        // Test music controls
        if (Input.GetKeyDown(KeyCode.O))
            AudioManager.Instance.PlayMusic("Main Menu Theme");

        if (Input.GetKeyDown(KeyCode.S))
            AudioManager.Instance.StopMusic();
    }
}