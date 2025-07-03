using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[System.Serializable]
public class DialogueLine
{
    public string characterName;
    public string dialogueText;
    public Sprite characterSprite; 
    public Sprite villianSprite;
    public float typingSpeed = 0.05f;
    public bool waitForInput = true;
}

[System.Serializable]
public class DialogueSequence
{
    public string sequenceName;
    public List<DialogueLine> dialogueLines;
}

public class CutsceneDialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI villianNameText;
    public TextMeshProUGUI dialogueText;
    public Image characterPortrait;
    public Image villianPortrait;
    public Button continueButton;
    public GameObject continuePrompt;

    [Header("Dialogue Data")]
    public DialogueSequence currentSequence;

    [Header("Settings")]
    public float defaultTypingSpeed = 0.05f;
    public bool canSkipTyping = true;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool isDialogueActive = false;
    private Coroutine typingCoroutine;

    public Action OnDialogueStart;
    public Action OnDialogueEnd;
    public Action<int> OnDialogueLineChanged;

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(NextLine);

        if (continuePrompt != null)
            continuePrompt.SetActive(false);
    }

    void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (isTyping && canSkipTyping)
            {
                SkipTyping();
            }
            else if (!isTyping)
            {
                NextLine();
            }
        }
    }

    public void StartDialogue(DialogueSequence sequence)
    {
        currentSequence = sequence;
        currentLineIndex = 0;
        isDialogueActive = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        OnDialogueStart?.Invoke();
        DisplayCurrentLine();
    }

    public void StartBossDialogue()
    {
        DialogueSequence bossSequence = new DialogueSequence
        {
            sequenceName = "Boss Encounter",
            dialogueLines = new List<DialogueLine>
            {
                new DialogueLine
                {
                    characterName = "Shadow Lord",
                    dialogueText = "Lol, whe tf is this?"
                },
                new DialogueLine
                {
                    characterName = "Hero",
                    dialogueText = "Yo. What is your favourite anime?",
                    typingSpeed = 0.03f,
                    characterSprite = characterPortrait.sprite,

                },
                new DialogueLine
                {
                    characterName = "Shadow Lord",
                    dialogueText = "I kind of like bleach and rent a girlfriend.",
                    typingSpeed = 0.04f,
                    villianSprite = villianPortrait.sprite
                },
                new DialogueLine
                {
                    characterName = "Hero",
                    dialogueText = "Mid.",
                    typingSpeed = 0.03f,
                    characterSprite = characterPortrait.sprite,
                },
                new DialogueLine
                {
                    characterName = "Shadow Lord",
                    dialogueText = "I am gonna destroy you.",
                    typingSpeed = 0.04f,
                    villianSprite = villianPortrait.sprite
                },
                new DialogueLine
                {
                    characterName = "Hero",
                    dialogueText = "Lol",
                    typingSpeed = 0.03f,
                    characterSprite = characterPortrait.sprite,
                },
                new DialogueLine
                {
                    characterName = "Shadow Lord",
                    dialogueText = "Prepare to meet your doom, little hero!",
                    typingSpeed = 0.04f,
                    villianSprite = villianPortrait.sprite
                }
            }
        };

        StartDialogue(bossSequence);
    }

    void DisplayCurrentLine()
    {
        if (currentLineIndex >= currentSequence.dialogueLines.Count)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = currentSequence.dialogueLines[currentLineIndex];

        if (characterNameText != null)
            characterNameText.text = currentLine.characterName;

        if (characterPortrait != null && currentLine.characterSprite != null)
        {
            characterPortrait.sprite = currentLine.characterSprite;
            characterPortrait.gameObject.SetActive(true);
        }
        else
        {
            characterPortrait.gameObject.SetActive(false);
        }

        if (villianPortrait != null && currentLine.villianSprite != null)
        {
            villianPortrait.sprite = currentLine.villianSprite;
            villianPortrait.gameObject.SetActive(true);
        }
        else
        {
            villianPortrait.gameObject.SetActive(false);
        }

        if (continuePrompt != null)
            continuePrompt.SetActive(false);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(currentLine.dialogueText, currentLine.typingSpeed));

        OnDialogueLineChanged?.Invoke(currentLineIndex);
    }

    IEnumerator TypeText(string text, float speed)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(speed);
        }

        isTyping = false;

        if (continuePrompt != null)
            continuePrompt.SetActive(true);
    }

    void SkipTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            DialogueLine currentLine = currentSequence.dialogueLines[currentLineIndex];
            dialogueText.text = currentLine.dialogueText;
            isTyping = false;

            if (continuePrompt != null)
                continuePrompt.SetActive(true);
        }
    }

    public void NextLine()
    {
        if (isTyping)
        {
            SkipTyping();
            return;
        }

        currentLineIndex++;
        DisplayCurrentLine();
    }

    void EndDialogue()
    {
        isDialogueActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        OnDialogueEnd?.Invoke();
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    public void PauseDialogue()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        isDialogueActive = false;
    }

    public void ResumeDialogue()
    {
        isDialogueActive = true;
        DisplayCurrentLine();
    }
}
