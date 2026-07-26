using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public class TutorialPage
{
    [TextArea(3, 8)] public string text;
    public string actionLabel = "Continue";
    public UnityEvent onAction;
}

[Serializable]
public class LevelTutorial
{
    [Tooltip("Shown the first time this level number is reached.")]
    public int level = 1;
    public List<TutorialPage> pages = new();
}

public class TutorialManager : Singleton<TutorialManager>, IPreBuildGate
{
    public int Order => 0;

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI pageText;
    [SerializeField] private TextMeshProUGUI actionButtonLabel;
    [SerializeField] private Button actionButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private float charactersPerSecond = 40f;
    [SerializeField] private List<LevelTutorial> levelTutorials = new();

    private readonly HashSet<int> seenLevels = new();
    private List<TutorialPage> activePages;
    private int activeLevel;
    private int currentPageIndex;
    private bool pageFullyRevealed;
    private Coroutine revealRoutine;
    private Action onComplete;

    private void OnEnable()
    {
        GameManager.Instance?.RegisterPreBuildGate(this);
    }

    private void OnDisable()
    {
        GameManager.Instance?.UnregisterPreBuildGate(this);
    }

    public IEnumerator WaitUntilReady(int level)
    {
        bool done = false;
        BeginForLevel(level, () => done = true);
        yield return new WaitUntil(() => done);
    }

    public void BeginForLevel(int level, Action onCompleteCallback)
    {
        onComplete = onCompleteCallback;

        var tutorial = levelTutorials.FirstOrDefault(t => t.level == level);
        bool masterEnabled = SettingsManager.Instance == null || SettingsManager.Instance.tutorialEnabled;
        bool shouldShow = tutorial != null && tutorial.pages.Count > 0 && masterEnabled && !seenLevels.Contains(level);

        if (!shouldShow)
        {
            InvokeComplete();
            return;
        }

        activePages = tutorial.pages;
        activeLevel = level;
        currentPageIndex = 0;
        if (panel != null) panel.SetActive(true);
        ShowPage(currentPageIndex);
    }

    private void Update()
    {
        if (panel == null || !panel.activeSelf || pageFullyRevealed) return;

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.anyKey.wasPressedThisFrame)
        {
            CompleteReveal();
        }
    }

    private void ShowPage(int index)
    {
        var page = activePages[index];
        pageText.text = page.text;
        pageText.maxVisibleCharacters = 0;
        pageFullyRevealed = false;

        if (actionButtonLabel != null)
            actionButtonLabel.text = string.IsNullOrEmpty(page.actionLabel) ? "Continue" : page.actionLabel;

        if (revealRoutine != null) StopCoroutine(revealRoutine);
        revealRoutine = StartCoroutine(RevealText());
    }

    private IEnumerator RevealText()
    {
        int totalCharacters = pageText.text.Length;
        float revealed = 0f;

        while (revealed < totalCharacters)
        {
            revealed += charactersPerSecond * Time.deltaTime;
            pageText.maxVisibleCharacters = Mathf.FloorToInt(revealed);
            yield return null;
        }

        CompleteReveal();
    }

    private void CompleteReveal()
    {
        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        pageText.maxVisibleCharacters = pageText.text.Length;
        pageFullyRevealed = true;
    }

    public void OnActionButtonClicked()
    {
        if (panel == null || !panel.activeSelf) return;

        if (!pageFullyRevealed)
        {
            CompleteReveal();
        }

        var page = activePages[currentPageIndex];
        page.onAction?.Invoke();

        currentPageIndex++;
        if (currentPageIndex >= activePages.Count)
        {
            FinishTutorial();
            return;
        }

        ShowPage(currentPageIndex);
    }

    public void OnSkipButtonClicked()
    {
        if (panel == null || !panel.activeSelf) return;

        if (revealRoutine != null)
        {
            StopCoroutine(revealRoutine);
            revealRoutine = null;
        }

        if (SettingsManager.Instance != null)
            SettingsManager.Instance.tutorialEnabled = false;

        panel.SetActive(false);
        seenLevels.Add(activeLevel);
        InvokeComplete();
    }

    private void FinishTutorial()
    {
        if (panel != null) panel.SetActive(false);

        seenLevels.Add(activeLevel);
        InvokeComplete();
    }

    private void InvokeComplete()
    {
        var callback = onComplete;
        onComplete = null;
        callback?.Invoke();
    }
}
