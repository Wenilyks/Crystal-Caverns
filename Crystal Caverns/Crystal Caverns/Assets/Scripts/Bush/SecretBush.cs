using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class SecretBush : MonoBehaviour
{
    [Header("Animation settings")]
    public float danceAmplitude = 0.3f;
    public float danceSpeed = 2f;
    public float detectionRange = 3f;

    [Header("Teleportation")]
    public Transform teleportDestination;
    public float fadeTransitionTime = 1.7f;

    [Header("UI")]
    public GameObject interactionPrompt;
    public KeyCode interactionKey = KeyCode.X;

    [Header("Effects")]
    public ParticleSystem magicalParticles;
    public AudioClip rustleSound;
    public AudioClip teleportSound;

    public float promptFadeSpeed = 2f;

    private Transform player;
    private Vector3 originalPosition;
    private bool playerInRange = false;
    private bool isAnimating = false;
    private bool secretDiscovered = false;
    private CanvasGroup fadePanel;
    private CanvasGroup promptCanvasGroup;


    private void Start()
    {
        originalPosition = transform.position;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        SetupFadePanel();

        if (interactionPrompt != null)
            promptCanvasGroup = interactionPrompt.GetComponent<CanvasGroup>();


        if (magicalParticles != null)
            magicalParticles.Stop();
    }

    private void Update()
    {
        if (player == null || secretDiscovered) return;

        CheckPlayerDistance();
        HandleAnimation();
        HandleInput();
    }

    private void CheckPlayerDistance()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool wasInRange = playerInRange;
        playerInRange = distanceToPlayer <= detectionRange;

        if (playerInRange && !wasInRange)
        {
            OnPlayerEnterRange();
        }

        else if (!playerInRange && wasInRange)
        {
            OnPlayerExitRange();
        }
    }

    private void OnPlayerEnterRange()
    {
        isAnimating = true;

        if (interactionPrompt != null)
            SetPromptVisibility(true);

        if (magicalParticles != null)
            magicalParticles.Play();
    }

    private void OnPlayerExitRange()
    {
        isAnimating = false;

        if (interactionPrompt != null)
            SetPromptVisibility(false);

        if (magicalParticles != null)
            magicalParticles.Stop();

        transform.position = originalPosition;
    }

    private void HandleAnimation()
    {
        if (!isAnimating) return;

        float sway = Mathf.Sin(Time.time * danceSpeed) * danceAmplitude;
        Vector3 newPosition = originalPosition + new Vector3(sway, sway * 0.5f, 0);
        transform.position = newPosition;
    }

    private void HandleInput()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            StartCoroutine(ActivateSecret());
        }
    }

    private void SetPromptVisibility(bool visible, bool immediate = false)
    {
        if (promptCanvasGroup == null) return;

        StopCoroutine("FadePrompt");
        StartCoroutine(FadePrompt(visible ? 1f : 0f, immediate));
    }

    private IEnumerator FadePrompt(float toAlpha, bool immediate)
    {
        if (immediate)
        {
            promptCanvasGroup.alpha = toAlpha;
            yield break;
        }

        float fromAlpha = promptCanvasGroup.alpha;

        float elapsed = 0f;

        while (elapsed < promptFadeSpeed)
        {
            elapsed += Time.deltaTime; 
            float t = elapsed / promptFadeSpeed;
            promptCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            yield return null;
        }

        promptCanvasGroup.alpha = toAlpha;
    }

    private IEnumerator ActivateSecret()
    {
        secretDiscovered = true;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        if (teleportSound != null)
            AudioManager.Instance.PlaySFX(teleportSound);

        if (magicalParticles != null)
        {
            var emission = magicalParticles.emission;
            emission.rateOverTime = 50f;
        }

        yield return StartCoroutine(FadeScreen(0f, 1f));

        if (teleportDestination != null)
            player.position = teleportDestination.position;

        yield return StartCoroutine(FadeScreen(1f, 0f));

        isAnimating = false;
        transform.position = originalPosition;

        if (magicalParticles != null)
            magicalParticles.Stop();
    }

    private void SetupFadePanel()
    {
        GameObject fadeGO = GameObject.Find("FadePanel");
        fadePanel = fadeGO.GetComponentInChildren<CanvasGroup>();
    }

    private IEnumerator FadeScreen(float fromAlpha, float toAlpha)
    {
        if (fadePanel == null) yield break;

        float elapsed = 0f;

        while (elapsed < fadeTransitionTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTransitionTime;
            fadePanel.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            yield return null;
        }

        fadePanel.alpha = toAlpha;
    }


}
