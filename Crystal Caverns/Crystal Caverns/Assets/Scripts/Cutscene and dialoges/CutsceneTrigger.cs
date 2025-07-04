using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    public CutsceneDialogueManager dialogueManager;
    public DialogueSequence customSequence;
    public bool triggerOnStart = false;
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    void Start()
    {
        if (triggerOnStart)
        {
            TriggerCutscene();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && (!hasTriggered || !triggerOnce))
        {
            TriggerCutscene();
        }
    }

    public void TriggerCutscene()
    {
        CameraController cameraController = Camera.main.GetComponent<CameraController>();
        if (cameraController != null)
        {
            Debug.Log("lol");
            cameraController.MoveCamera();
        }


        if (dialogueManager != null)
        {
            if (customSequence != null && customSequence.dialogueLines.Count > 0)
            {
                dialogueManager.StartDialogue(customSequence);
            }
            else
            {
                dialogueManager.StartBossDialogue();
            }

            hasTriggered = true;
        }
    }
}