using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.EventSystems;



public class MapSelectionManager : MonoBehaviour
{
    [Header("Map Configuration")]
    public List<MapData> maps = new List<MapData>();
    public Transform mapContainer;

    [Header("UI Elements")]
    public ScrollRect scrollRect;
    public Button nextMapButton;
    public Button prevMapButton;
    public Button playButton;
    public TMP_Text mapNameText;
    public TMP_Text mapDescriptionText;
    public GameObject backgroundObj;
    public Transform informationPanel;

    [Header("Animation Settings")]
    public float transitionDuration = 0.5f;
    public Ease transitionEase = Ease.OutCubic;
    public float buttonAnimationDuration = 0.3f;

    [Header("Visual Effects")]
    public AudioSource audioSource;
    public AudioClip buttonClickSound;
    public AudioClip mapChangeSound;

    private int currentMapIndex = 0;
    private List<MapDisplay> mapDisplays = new List<MapDisplay>();

    void Start()
    {
        LoadPlayerProgress();
        SetupMaps();
        ShowMap(0);
        SetupUI();
    }

    void LoadPlayerProgress()
    {
        // Loading player progress
        for (int i = 0; i < maps.Count; i++)
        {
            string unlockKey = $"Map_{i}_{maps[i].mapName}_unlocked";
            maps[i].isUnlocked = PlayerPrefs.GetInt(unlockKey, maps[i].isUnlocked ? 1 : 0) == 1;
        }
    }

    void SetupMaps()
    {
        for (int i = 0; i < maps.Count; i++)
        {
            GameObject mapObj = new GameObject($"Map_{i}_{maps[i].mapName}");
            mapObj.transform.SetParent(mapContainer);
            mapObj.transform.localScale = Vector3.one;

            MapDisplay mapDisplay = mapObj.AddComponent<MapDisplay>();
            mapDisplay.Initialize(maps[i], this, backgroundObj, mapContainer, informationPanel);
            mapDisplays.Add(mapDisplay);

            // Position maps horizontally
            RectTransform rectTransform = mapObj.GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = mapObj.AddComponent<RectTransform>();

            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            mapObj.SetActive(false);
        }
    }

    void SetupUI()
    {
        if (nextMapButton != null)
        {
            nextMapButton.onClick.AddListener(() => ChangeMap(1));
        }

        if (prevMapButton != null)
        {
            prevMapButton.onClick.AddListener(() => ChangeMap(-1));
        }

        if (playButton != null)
        {
            playButton.onClick.AddListener(PlayCurrentMap);
        }

        UpdateUI();
    }

    public void ChangeMap(int direction)
    {
        PlaySound(mapChangeSound);

        int newIndex = currentMapIndex + direction;
        newIndex = Mathf.Clamp(newIndex, 0, maps.Count - 1);

        if (newIndex != currentMapIndex)
        {
            ShowMap(newIndex);
        }
    }

    void ShowMap(int mapIndex)
    {
        if (mapIndex < 0 || mapIndex >= maps.Count) return;

        StartCoroutine(TransitionToMap(mapIndex));
    }

    IEnumerator TransitionToMap(int newMapIndex)
    {
        int oldMapIndex = currentMapIndex;

        if (oldMapIndex >= 0 && oldMapIndex < mapDisplays.Count)
        {
            string outDirection = oldMapIndex < newMapIndex ? "left" : "right";
            yield return StartCoroutine(mapDisplays[oldMapIndex].PlayBackgroundOutAnimation(outDirection));
        }

        // Hide current map
        if (oldMapIndex >= 0 && oldMapIndex < mapDisplays.Count)
        {
            if (oldMapIndex < newMapIndex)
                yield return StartCoroutine(AnimateMapOut(mapDisplays[oldMapIndex], "left"));
            else
                yield return StartCoroutine(AnimateMapOut(mapDisplays[oldMapIndex], "right"));
        }

        currentMapIndex = newMapIndex;

        // Show new map
        if (oldMapIndex < newMapIndex)
            yield return StartCoroutine(AnimateMapIn(mapDisplays[currentMapIndex], "right"));
        else
            yield return StartCoroutine(AnimateMapIn(mapDisplays[currentMapIndex], "left"));

        UpdateUI();
    }

    IEnumerator AnimateMapOut(MapDisplay mapDisplay, string side)
    {
        Transform mapTransform = mapDisplay.transform;

        // Slide out to the specified side with scale down
        if (side == "left")
            mapTransform.DOLocalMoveX(-Screen.width, transitionDuration).SetEase(transitionEase);
        else
            mapTransform.DOLocalMoveX(Screen.width, transitionDuration).SetEase(transitionEase);

        mapTransform.DOScale(0.8f, transitionDuration).SetEase(transitionEase);

        // Fade out
        CanvasGroup canvasGroup = mapDisplay.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = mapDisplay.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.DOFade(0f, transitionDuration).SetEase(transitionEase);

        if (mapNameText != null)
        {
            mapNameText.DOFade(0f, transitionDuration * 0.5f).SetEase(transitionEase);
        }
        if (mapDescriptionText != null)
        {
            mapDescriptionText.DOFade(0f, transitionDuration * 0.5f).SetEase(transitionEase);
        }

        yield return new WaitForSeconds(transitionDuration);
        mapDisplay.gameObject.SetActive(false);
    }

    IEnumerator AnimateMapIn(MapDisplay mapDisplay, string side)
    {
        if (mapDisplay == null) yield break;

        mapDisplay.gameObject.SetActive(true);

        mapDisplay.UpdateBackground();

        Transform mapTransform = mapDisplay.transform;

        float startX = (side == "left") ? -Screen.width : Screen.width;
        mapTransform.localPosition = new Vector3(startX, 0, 0);
        mapTransform.localScale = Vector3.one * 0.8f;

        CanvasGroup canvasGroup = mapDisplay.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = mapDisplay.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;

        if (mapNameText != null)
        {
            mapNameText.color = new Color(mapNameText.color.r, mapNameText.color.g, mapNameText.color.b, 0f);
        }
        if (mapDescriptionText != null)
        {
            mapDescriptionText.color = new Color(mapDescriptionText.color.r, mapDescriptionText.color.g, mapDescriptionText.color.b, 0f);
        }

        UpdateUIContent();

        mapTransform.DOLocalMoveX(0, transitionDuration).SetEase(transitionEase);
        mapTransform.DOScale(1f, transitionDuration).SetEase(transitionEase);
        canvasGroup.DOFade(1f, transitionDuration).SetEase(transitionEase);

        if (mapNameText != null)
        {
            mapNameText.DOFade(1f, transitionDuration * 0.7f)
                .SetDelay(transitionDuration * 0.3f)
                .SetEase(transitionEase);
        }
        if (mapDescriptionText != null)
        {
            mapDescriptionText.DOFade(1f, transitionDuration * 0.7f)
                .SetDelay(transitionDuration * 0.4f)
                .SetEase(transitionEase);
        }

        yield return new WaitForSeconds(transitionDuration);
        mapDisplay.PlayEntranceAnimation();
    }

    void UpdateUIContent()
    {
        if (currentMapIndex >= 0 && currentMapIndex < maps.Count)
        {
            MapData currentMap = maps[currentMapIndex];

            if (mapNameText != null)
            {
                mapNameText.text = currentMap.mapName;
            }

            if (mapDescriptionText != null)
            {
                mapDescriptionText.text = currentMap.mapDescription;
            }

            UpdatePlayButtonState(currentMap);
        }
    }

    void UpdateUI()
    {
        UpdateUIContent();
        UpdateNavigationButtons();
    }

    void UpdatePlayButtonState(MapData currentMap)
    {
        if (playButton != null)
        {
            playButton.interactable = currentMap.isUnlocked;

            Text buttonText = playButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = currentMap.isUnlocked ? "PLAY" : "LOCKED";
            }

            Image buttonImage = playButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = currentMap.isUnlocked ? Color.white : Color.gray;
            }
        }
    }
    void UpdateNavigationButtons()
    {
        if (prevMapButton != null)
        {
            prevMapButton.interactable = currentMapIndex > 0;

            Image prevImage = prevMapButton.GetComponent<Image>();
            if (prevImage != null)
                prevImage.color = prevMapButton.interactable ? Color.white : Color.gray;
        }

        if (nextMapButton != null)
        {
            nextMapButton.interactable = currentMapIndex < maps.Count - 1;

            Image nextImage = nextMapButton.GetComponent<Image>();
            if (nextImage != null)
                nextImage.color = nextMapButton.interactable ? Color.white : Color.gray;
        }
    }

    void PlayCurrentMap()
    {
        if (currentMapIndex >= 0 && currentMapIndex < maps.Count)
        {
            MapData currentMap = maps[currentMapIndex];

            if (!currentMap.isUnlocked)
            {
                Debug.Log("Map is locked!");
                return;
            }

            PlaySound(buttonClickSound);

            StartCoroutine(LoadMapWithTransition(currentMap.sceneName));
        }
    }

    IEnumerator LoadMapWithTransition(string sceneName)
    {
        // We can add loading animation here

        yield return new WaitForSeconds(0.5f);

        SceneLoader.Instance.LoadScene(sceneName);
    }

    void PlaySound(AudioClip clip)
    {
        AudioManager.Instance.PlaySFX(clip);
    }

    // We are calling this method when a map is completed to update progress
    public void CompleteMapWithStars(int mapIndex, int starsEarned)
    {
        if (mapIndex >= 0 && mapIndex < maps.Count)
        {
            maps[mapIndex + 1].isUnlocked = true;
            SavePlayerProgress();
        }
    }

    public void SavePlayerProgress()
    {
        for (int i = 0; i < maps.Count; i++)
        {
            string unlockKey = $"Map_{i}_{maps[i].mapName}_unlocked";
            PlayerPrefs.SetInt(unlockKey, maps[i].isUnlocked ? 1 : 0);
        }

        PlayerPrefs.Save();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SavePlayerProgress();
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SavePlayerProgress();
    }
}
