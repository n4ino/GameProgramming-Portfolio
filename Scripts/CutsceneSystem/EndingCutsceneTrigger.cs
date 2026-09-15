using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

/*
* EndingCutsceneTrigger
*
* Triggers the game's ending cutscene when the player
* reaches the ending area. Supports multiple endings,
* credits display, optional credits music, and
* automatic return to the main menu.
*/

[RequireComponent(typeof(Collider2D))]
public class EndingCutsceneTrigger : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private VideoClip normalEndingClip;
    [SerializeField] private VideoClip completionEndingClip;

    [Header("Completion Requirements")]
    [SerializeField] private int requiredRunestones = 5;
    [SerializeField] private int requiredSkills = 4;

    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool playOnlyOnce = true;

    [Header("Credits")]
    [SerializeField] private Sprite normalCreditsImage;
    [SerializeField] private Sprite completionCreditsImage;
    [SerializeField, Min(0f)] private float creditsDuration = 45f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private Color creditsBackgroundColor = Color.black;

    [Header("Credits Music (optional)")]
    [Tooltip("Music played during the credits screen. Leave empty to disable.")]
    [SerializeField] private AudioClip creditsMusic;
    [Tooltip("Credits music volume (0-1).")]
    [Range(0f, 1f)]
    [SerializeField] private float creditsMusicVolume = 0.8f;
    [Tooltip("Loop the music if the credits duration exceeds the clip length.")]
    [SerializeField] private bool loopCreditsMusic = true;
    [Tooltip("Mute gameplay AudioSources while the credits screen is active.")]
    [SerializeField] private bool muteGameAudioDuringCredits = true;

    private bool hasPlayed;
    private bool currentEndingIsCompletion;

    private void Reset()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Prevent the ending from being triggered multiple times.
        if (hasPlayed && playOnlyOnce)
            return;

        if (!other.CompareTag(playerTag))
            return;

        PlayEnding();
    }

    public void PlayEnding()
    {
        if (CutsceneManager.Instance == null)
        {
            Debug.LogWarning("[EndingCutsceneTrigger] CutsceneManager.Instance is missing from the scene.", this);
            return;
        }

        // Determine which ending should be played based on player progress.
        bool hasCompletionEnding = GameProgress.Instance != null &&
                                   GameProgress.Instance.HasTrueEndingRequirements(requiredRunestones, requiredSkills);

        VideoClip clip = hasCompletionEnding ? completionEndingClip : normalEndingClip;

        if (clip == null)
        {
            Debug.LogWarning("[EndingCutsceneTrigger] Ending clip is missing. Completion ending = " + hasCompletionEnding, this);
            return;
        }

        currentEndingIsCompletion = hasCompletionEnding;
        hasPlayed = true;

        // Play the selected ending and show credits when it finishes.
        CutsceneManager.Instance.PlayCutscene(clip, ShowCreditsThenReturnToMainMenu);
    }

    private void ShowCreditsThenReturnToMainMenu()
    {
        StartCoroutine(CreditsRoutine(currentEndingIsCompletion ? completionCreditsImage : normalCreditsImage));
    }

    private IEnumerator CreditsRoutine(Sprite creditsSprite)
    {
        GameTime.SetCutscene(true);

        // Stop zone music so it does not continue into the credits or main menu.
        AudioZoneManager.Instance?.StopAllMusic(fade: false);

        GameObject canvasObject = new GameObject(
            "EndingCreditsCanvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));

        DontDestroyOnLoad(canvasObject);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Image background = CreateFullscreenImage("Background", canvasObject.transform);
        background.color = creditsBackgroundColor;

        if (creditsSprite != null)
        {
            Image credits = CreateFullscreenImage("CreditsImage", canvasObject.transform);
            credits.sprite = creditsSprite;
            credits.color = Color.white;
            credits.preserveAspect = true;
        }

        // Create a dedicated AudioSource for the credits music.
        // The canvas persists across scene loads through DontDestroyOnLoad.
        AudioSource creditsAudio = null;
        if (creditsMusic != null)
        {
            creditsAudio = canvasObject.AddComponent<AudioSource>();
            creditsAudio.clip = creditsMusic;
            creditsAudio.loop = loopCreditsMusic;
            creditsAudio.volume = creditsMusicVolume;

            // Continue playing even when gameplay is paused.
            creditsAudio.ignoreListenerPause = true;
            creditsAudio.playOnAwake = false;
        }

        // Mute gameplay audio while the credits music is playing.
        if (muteGameAudioDuringCredits)
            GameAudioMute.Mute(creditsAudio);

        if (creditsAudio != null)
            creditsAudio.Play();

        yield return new WaitForSecondsRealtime(creditsDuration);

        // Clean up credits audio and restore normal gameplay state.
        if (creditsAudio != null)
            creditsAudio.Stop();

        if (muteGameAudioDuringCredits)
            GameAudioMute.Restore();

        GameTime.SetCutscene(false);
        SceneManager.LoadScene(mainMenuSceneName);
        Destroy(canvasObject);
    }

    // Create a fullscreen UI image used for the credits screen.
    private static Image CreateFullscreenImage(string objectName, Transform parent)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        return imageObject.GetComponent<Image>();
    }
}
