using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button exitGameButton;

    private void Start()
    {
        SetupButtons();
    }

    private void SetupButtons()
    {
        startGameButton.onClick.AddListener(() => { SceneLoader.Instance.LoadScene("ChooseLevel"); AudioManager.Instance.PlaySFX("Button click"); });
        settingsButton.onClick.AddListener(() => { AudioManager.Instance.PlaySFX("Button click"); });
        creditsButton.onClick.AddListener(() => { AudioManager.Instance.PlaySFX("Button click"); });
        exitGameButton.onClick.AddListener(() => { AudioManager.Instance.PlaySFX("Button click"); });
        
    }
}