using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections;
public class MapDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private MapData mapData;
    private MapSelectionManager mapManager;
    private Image backgroundImage;
    private Image previewImage;
    private Image background;
    private GameObject backgroundObj;
    private Transform mapContainer;
    private Transform informationPanel;

    [Header("Background Animation Settings")]
    public BackgroundAnimationType backgroundAnimationType = BackgroundAnimationType.FadeIn;
    public float backgroundAnimationDuration = 1.0f;
    public float backgroundAnimationDelay = 0.3f;
    public Ease backgroundAnimationEase = Ease.OutQuart;

    [Header("Background Out Animation Settings")]
    public float backgroundOutAnimationDuration = 0.8f;
    public Ease backgroundOutAnimationEase = Ease.InQuart;

    public enum BackgroundAnimationType
    {
        FadeIn,
        SlideFromLeft,
        SlideFromRight,
        SlideFromTop,
        SlideFromBottom,
        ScaleUp,
        RotateIn,
        ZoomAndFade,
        WaveEffect,
        SpiralIn
    }

    private bool isMovedLeft = false;
    private Vector3 originalPosition;
    private Vector3 infoOriginalPosition;
    private bool isAnimating = false;

    void Start()
    {
        if (previewImage != null)
        {
            originalPosition = previewImage.transform.localPosition;
        }
    }

    public void Initialize(MapData data, MapSelectionManager manager, GameObject bg, Transform mc, Transform infoPanel)
    {
        mapData = data;
        mapManager = manager;
        backgroundObj = bg;
        mapContainer = mc;
        informationPanel = infoPanel;
        infoOriginalPosition = infoPanel.localPosition;

        SetupMapVisuals();
    }

    void SetupMapVisuals()
    {
        // Set main background image
        backgroundImage = GetComponent<Image>();
        if (backgroundImage == null)
            backgroundImage = gameObject.AddComponent<Image>();


        if (mapData.mapBackground != null)
            backgroundImage.sprite = mapData.mapBackground;

        backgroundImage.color = mapData.mapThemeColor;

        // Setup the shared background object (don't set sprite here, do it in UpdateBackground)
        if (backgroundObj != null)
        {
            background = backgroundObj.GetComponent<Image>();
            if (background == null)
            {
                background = backgroundObj.AddComponent<Image>();
            }

            RectTransform backgroundRect = backgroundObj.GetComponent<RectTransform>();
            if (backgroundRect == null)
            {
                backgroundRect = backgroundObj.AddComponent<RectTransform>();
            }

            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
        }

        if (mapData.mapPreview != null)
        {
            GameObject previewObj = new GameObject("MapPreview");
            previewObj.transform.SetParent(transform);

            previewImage = previewObj.AddComponent<Image>();
            previewImage.sprite = mapData.mapPreview;

            RectTransform previewRect = previewObj.GetComponent<RectTransform>();
            previewRect.anchorMin = new Vector2(0.1f, 0.1f);
            previewRect.anchorMax = new Vector2(0.9f, 0.9f);
            previewRect.offsetMin = Vector2.zero;
            previewRect.offsetMax = Vector2.zero;

            previewImage.color = mapData.isUnlocked ? Color.white : Color.black;
            originalPosition = previewImage.transform.localPosition;
        }
    }

    public void UpdateBackground()
    {
        if (background != null && mapData.background != null)
        {
            background.sprite = mapData.background;
            SetupBackgroundForAnimation();
            Debug.Log($"Updated background to: {mapData.mapName} - {mapData.background.name}");
        }
        else
        {
            Debug.Log($"Cannot update background - background: {background != null}, sprite: {mapData.background != null}");
        }
    }

    void SetupBackgroundForAnimation()
    {
        if (background == null) return;

        switch (backgroundAnimationType)
        {
            case BackgroundAnimationType.FadeIn:
                background.color = new Color(background.color.r, background.color.g, background.color.b, 0f);
                break;

            case BackgroundAnimationType.SlideFromLeft:
                background.transform.localPosition = new Vector3(-Screen.width, 0, 0);
                break;

            case BackgroundAnimationType.SlideFromRight:
                background.transform.localPosition = new Vector3(Screen.width, 0, 0);
                break;

            case BackgroundAnimationType.SlideFromTop:
                background.transform.localPosition = new Vector3(0, Screen.height, 0);
                break;

            case BackgroundAnimationType.SlideFromBottom:
                background.transform.localPosition = new Vector3(0, -Screen.height, 0);
                break;

            case BackgroundAnimationType.ScaleUp:
                background.transform.localScale = Vector3.zero;
                break;

            case BackgroundAnimationType.RotateIn:
                background.transform.localScale = Vector3.zero;
                background.transform.rotation = Quaternion.Euler(0, 0, 180f);
                break;

            case BackgroundAnimationType.ZoomAndFade:
                background.transform.localScale = Vector3.one * 2f;
                background.color = new Color(background.color.r, background.color.g, background.color.b, 0f);
                break;

            case BackgroundAnimationType.WaveEffect:
                background.color = new Color(background.color.r, background.color.g, background.color.b, 0f);
                background.transform.localScale = new Vector3(0.1f, 1f, 1f);
                break;

            case BackgroundAnimationType.SpiralIn:
                background.transform.localScale = Vector3.zero;
                background.transform.rotation = Quaternion.Euler(0, 0, 360f);
                background.color = new Color(background.color.r, background.color.g, background.color.b, 0f);
                break;
        }
    }

    public void PlayEntranceAnimation()
    {
        // Update background first, then animate
        UpdateBackground();

        // Animate the background first
        PlayBackgroundAnimation();

        // Then animate other elements with slight delays
        StartCoroutine(AnimateOtherElements());
    }

    void PlayBackgroundAnimation()
    {
        if (background == null) return;

        DG.Tweening.Sequence backgroundSequence = DOTween.Sequence();

        switch (backgroundAnimationType)
        {
            case BackgroundAnimationType.FadeIn:
                backgroundSequence.Append(
                    background.DOFade(1f, backgroundAnimationDuration)
                        .SetEase(backgroundAnimationEase)
                        .SetDelay(backgroundAnimationDelay)
                );
                break;

            case BackgroundAnimationType.SlideFromLeft:
            case BackgroundAnimationType.SlideFromRight:
            case BackgroundAnimationType.SlideFromTop:
            case BackgroundAnimationType.SlideFromBottom:
                backgroundSequence.Append(
                    background.transform.DOLocalMove(Vector3.zero, backgroundAnimationDuration)
                        .SetEase(backgroundAnimationEase)
                        .SetDelay(backgroundAnimationDelay)
                );
                break;

            case BackgroundAnimationType.ScaleUp:
                backgroundSequence.Append(
                    background.transform.DOScale(Vector3.one, backgroundAnimationDuration)
                        .SetEase(backgroundAnimationEase)
                        .SetDelay(backgroundAnimationDelay)
                );
                break;

            case BackgroundAnimationType.RotateIn:
                backgroundSequence.Append(
                    background.transform.DOScale(Vector3.one, backgroundAnimationDuration)
                        .SetEase(backgroundAnimationEase)
                        .SetDelay(backgroundAnimationDelay)
                );
                backgroundSequence.Join(
                    background.transform.DORotate(Vector3.zero, backgroundAnimationDuration)
                        .SetEase(backgroundAnimationEase)
                        .SetDelay(backgroundAnimationDelay)
                );
                break;

            case BackgroundAnimationType.ZoomAndFade:
                backgroundSequence.Append(
                    background.transform.DOScale(Vector3.one, backgroundAnimationDuration)
                        .SetEase(backgroundAnimationEase)
                        .SetDelay(backgroundAnimationDelay)
                );
                backgroundSequence.Join(
                    background.DOFade(1f, backgroundAnimationDuration)
                        .SetEase(backgroundAnimationEase)
                        .SetDelay(backgroundAnimationDelay)
                );
                break;

            case BackgroundAnimationType.WaveEffect:
                backgroundSequence.Append(
                    background.DOFade(1f, backgroundAnimationDuration * 0.3f)
                        .SetEase(backgroundAnimationEase)
                        .SetDelay(backgroundAnimationDelay)
                );
                backgroundSequence.Append(
                    background.transform.DOScaleX(1f, backgroundAnimationDuration * 0.7f)
                        .SetEase(Ease.OutElastic)
                );
                break;

            case BackgroundAnimationType.SpiralIn:
                backgroundSequence.Append(
                    background.transform.DOScale(Vector3.one, backgroundAnimationDuration)
                        .SetEase(backgroundAnimationEase)
                        .SetDelay(backgroundAnimationDelay)
                );
                backgroundSequence.Join(
                    background.transform.DORotate(Vector3.zero, backgroundAnimationDuration)
                        .SetEase(backgroundAnimationEase)
                        .SetDelay(backgroundAnimationDelay)
                );
                backgroundSequence.Join(
                    background.DOFade(1f, backgroundAnimationDuration * 0.6f)
                        .SetEase(backgroundAnimationEase)
                        .SetDelay(backgroundAnimationDelay + backgroundAnimationDuration * 0.2f)
                );
                break;
        }

        backgroundSequence.Play();
    }

    public IEnumerator PlayBackgroundOutAnimation(string direction)
    {
        if (background == null) yield break;

        DG.Tweening.Sequence backgroundOutSequence = DOTween.Sequence();

        switch (backgroundAnimationType)
        {
            case BackgroundAnimationType.FadeIn:
                backgroundOutSequence.Append(
                    background.DOFade(0f, backgroundOutAnimationDuration)
                        .SetEase(backgroundOutAnimationEase)
                );
                break;

            case BackgroundAnimationType.SlideFromLeft:
                float targetX = direction == "left" ? -Screen.width : Screen.width;
                backgroundOutSequence.Append(
                    background.transform.DOLocalMoveX(targetX, backgroundOutAnimationDuration)
                        .SetEase(backgroundOutAnimationEase)
                );
                break;

            case BackgroundAnimationType.SlideFromRight:
                targetX = direction == "left" ? -Screen.width : Screen.width;
                backgroundOutSequence.Append(
                    background.transform.DOLocalMoveX(targetX, backgroundOutAnimationDuration)
                        .SetEase(backgroundOutAnimationEase)
                );
                break;

            case BackgroundAnimationType.SlideFromTop:
                float targetY = direction == "left" ? -Screen.height : Screen.height;
                backgroundOutSequence.Append(
                    background.transform.DOLocalMoveY(targetY, backgroundOutAnimationDuration)
                        .SetEase(backgroundOutAnimationEase)
                );
                break;

            case BackgroundAnimationType.SlideFromBottom:
                targetY = direction == "left" ? Screen.height : -Screen.height;
                backgroundOutSequence.Append(
                    background.transform.DOLocalMoveY(targetY, backgroundOutAnimationDuration)
                        .SetEase(backgroundOutAnimationEase)
                );
                break;

            case BackgroundAnimationType.ScaleUp:
                backgroundOutSequence.Append(
                    background.transform.DOScale(Vector3.zero, backgroundOutAnimationDuration)
                        .SetEase(backgroundOutAnimationEase)
                );
                break;

            case BackgroundAnimationType.RotateIn:
                backgroundOutSequence.Append(
                    background.transform.DOScale(Vector3.zero, backgroundOutAnimationDuration)
                        .SetEase(backgroundOutAnimationEase)
                );
                backgroundOutSequence.Join(
                    background.transform.DORotate(new Vector3(0, 0, direction == "left" ? -180f : 180f), backgroundOutAnimationDuration)
                        .SetEase(backgroundOutAnimationEase)
                );
                break;

            case BackgroundAnimationType.ZoomAndFade:
                backgroundOutSequence.Append(
                    background.transform.DOScale(Vector3.one * 0.5f, backgroundOutAnimationDuration)
                        .SetEase(backgroundOutAnimationEase)
                );
                backgroundOutSequence.Join(
                    background.DOFade(0f, backgroundOutAnimationDuration)
                        .SetEase(backgroundOutAnimationEase)
                );
                break;

            case BackgroundAnimationType.WaveEffect:
                backgroundOutSequence.Append(
                    background.transform.DOScaleX(0.1f, backgroundOutAnimationDuration * 0.7f)
                        .SetEase(Ease.InElastic)
                );
                backgroundOutSequence.Append(
                    background.DOFade(0f, backgroundOutAnimationDuration * 0.3f)
                        .SetEase(backgroundOutAnimationEase)
                );
                break;

            case BackgroundAnimationType.SpiralIn:
                backgroundOutSequence.Append(
                    background.transform.DOScale(Vector3.zero, backgroundOutAnimationDuration)
                        .SetEase(backgroundOutAnimationEase)
                );
                backgroundOutSequence.Join(
                    background.transform.DORotate(new Vector3(0, 0, direction == "left" ? -360f : 360f), backgroundOutAnimationDuration)
                        .SetEase(backgroundOutAnimationEase)
                );
                backgroundOutSequence.Join(
                    background.DOFade(0f, backgroundOutAnimationDuration * 0.8f)
                        .SetEase(backgroundOutAnimationEase)
                        .SetDelay(backgroundOutAnimationDuration * 0.1f)
                );
                break;
        }

        backgroundOutSequence.Play();
        yield return new WaitForSeconds(backgroundOutAnimationDuration);
    }

    System.Collections.IEnumerator AnimateOtherElements()
    {
        yield return new WaitForSeconds(backgroundAnimationDelay + backgroundAnimationDuration * 0.3f);

        // Bounce effect for the whole map
        transform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 10, 1f);

        // Animate preview image if it exists
        if (previewImage != null)
        {
            previewImage.transform.localScale = Vector3.zero;
            previewImage.transform.DOScale(1f, 0.6f)
                .SetEase(Ease.OutElastic)
                .SetDelay(0.2f);
        }

        // Color pulse effect for main background
        if (backgroundImage != null)
        {
            Color originalColor = backgroundImage.color;
            backgroundImage.color = Color.white;
            backgroundImage.DOColor(originalColor, 0.5f);
        }
    }

    // Method to set animation type from external scripts
    public void SetBackgroundAnimationType(BackgroundAnimationType animationType)
    {
        backgroundAnimationType = animationType;
    }

    // Method to set custom animation parameters
    public void SetBackgroundAnimationParameters(float duration, float delay, Ease ease)
    {
        backgroundAnimationDuration = duration;
        backgroundAnimationDelay = delay;
        backgroundAnimationEase = ease;
    }

    // Method to set custom out animation parameters
    public void SetBackgroundOutAnimationParameters(float duration, Ease ease)
    {
        backgroundOutAnimationDuration = duration;
        backgroundOutAnimationEase = ease;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        previewImage.transform.DOScale(1.1f, 0.6f);
        previewImage.color = Color.Lerp(previewImage.color, Color.grey, 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        previewImage.transform.DOScale(1f, 0.6f);
        previewImage.color = Color.Lerp(previewImage.color, Color.white, 0.5f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (previewImage == null || isAnimating) return;

        isAnimating = true;

        previewImage.transform.DOScale(1f, 0.6f);
        previewImage.color = Color.Lerp(previewImage.color, Color.white, 0.5f);

        if (!isMovedLeft)
        {
            // Move to left side (0.3f means 30% of screen width)
            Vector3 leftPosition = originalPosition + new Vector3(-Screen.width * 0.3f, 0, 0);
            mapContainer.DOLocalMove(leftPosition, 0.5f)
                .SetEase(Ease.OutQuart)
                .OnComplete(() => {
                    isMovedLeft = true;
                    isAnimating = false;
                });
            // Move info panel to the right side (very hard coded sorry) 
            Vector3 rightPosition = originalPosition - new Vector3(-Screen.width * 0.24f, 0, 0);
            informationPanel.DOLocalMove(rightPosition, 0.5f)
                .SetEase(Ease.OutQuart)
                .OnComplete(() => {
                    isMovedLeft = true;
                    isAnimating = false;
                });
        }
        else
        {
            // Move back to original position
            mapContainer.DOLocalMove(originalPosition, 0.5f)
                .SetEase(Ease.OutQuart)
                .OnComplete(() => {
                    isMovedLeft = false;
                    isAnimating = false;
                });

            // Move info panel to the original position 
            informationPanel.DOLocalMove(infoOriginalPosition, 0.5f)
                .SetEase(Ease.OutQuart)
                .OnComplete(() => {
                    isMovedLeft = false;
                    isAnimating = false;
                });
        }
    }

}